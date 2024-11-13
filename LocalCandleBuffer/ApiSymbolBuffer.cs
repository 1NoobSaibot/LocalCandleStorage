namespace LocalCandleBuffer
{
	public interface ICandleWritterReader<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		void WriteSingleCandleToFiler(TCandle candle, BinaryWriter writer);
		TCandle ReadSingleCandleFromFile(BinaryReader reader);
	}


	public abstract class ApiSymbolBufferV2<TCandle>
		: ICandleSource<TCandle>, ICandleWritterReader<TCandle>
		where TCandle : IStorableCandle<TCandle>
	{
		private const string RANGES_FILE_NAME = "RangesInfo.bin";
		private readonly string _root;
		private readonly ICandleSource<TCandle> _alternativeSource;
		private readonly BufferManifest<TCandle> _safeRanges;


		public ApiSymbolBufferV2(string root, ICandleSource<TCandle> alternativeSource)
		{
			_root = root;
			_alternativeSource = alternativeSource;
			_safeRanges = new(Path.Combine(_root, RANGES_FILE_NAME));
			Directory.CreateDirectory(_root);
		}


		public async Task<Fragment<TCandle>> Get1mCandles(
			string symbol,
			CandleRange? req,
			Action<int, int>? tellProgress = null
		)
		{
			req ??= CandleRange.AllByNow();
			await BufferizeMissedCandlesIfNeed(symbol, req, tellProgress);
			var candles = GetFromBuffer(symbol, req);
			Fragment<TCandle> frag = new(candles.ToArray(), TimeFrame.OneMinute);
			return frag;
		}


		public abstract void WriteSingleCandleToFiler(TCandle candle, BinaryWriter writer);
		public abstract TCandle ReadSingleCandleFromFile(BinaryReader reader);


		private async Task BufferizeMissedCandlesIfNeed(
			string symbol,
			CandleRange req,
			Action<int, int>? tellProgress = null
		)
		{
			TimeSpan chunkSize = TimeSpan.FromDays(30);
			var maybeRange = _safeRanges.GetBufferedRangeData(symbol);
			if (maybeRange is null)
			{
				// Load All from internet
				await LoadPartially(symbol, req.ToDescendingChunks(chunkSize), tellProgress);
				return;
			}

			CandleRange oldRange = maybeRange;
			if (req.StartUTC < oldRange.StartUTC)
			{
				CandleRange tail = new(req.StartUTC, oldRange.StartUTC);
				await LoadPartially(symbol, tail.ToDescendingChunks(chunkSize), tellProgress);
			}
			if (req.EndUTC > oldRange.EndUTC)
			{
				CandleRange head = new(oldRange.EndUTC, req.EndUTC);
				await LoadPartially(symbol, head.ToAscendingChunks(chunkSize), tellProgress);
			}
		}


		private async Task LoadPartially(
			string symbol,
			IList<CandleRange> ranges,
			Action<int, int>? tellProgress
		)
		{
			foreach (var range in ranges)
			{
				var candles = await _alternativeSource.Get1mCandles(symbol, range, tellProgress);
				Save(symbol, candles.GetCandles());
			}
		}


		private IList<TCandle> GetFromBuffer(string symbol, CandleRange req)
		{
			var storage = SymbolToStorage(symbol);
			var candles = storage.Read(req);
			return candles;
		}


		private void Save(string symbol, IList<TCandle> candlesToSave)
		{
			var storage = SymbolToStorage(symbol);
			storage.Save(candlesToSave);
			_safeRanges.UpdateRangeData(symbol, candlesToSave);
		}


		private FragmentaryCandleStorage<TCandle> SymbolToStorage(string symbol)
		{
			string storagePath = Path.Combine(_root, symbol);
			return new FragmentaryCandleStorage<TCandle>(this, storagePath);
		}
	}
}
