namespace LocalCandleBuffer
{
	public class ApiSymbolBufferV2 : ICandleSource
	{
		private const string RANGES_FILE_NAME = "RangesInfo.bin";
		private readonly string _root;
		private readonly ICandleSource _alternativeSource;
		private readonly BufferManifest _safeRanges;

		public ApiSymbolBufferV2(string root, ICandleSource alternativeSource)
		{
			_root = root;
			_alternativeSource = alternativeSource;
			_safeRanges = new(Path.Combine(_root, RANGES_FILE_NAME));
			Directory.CreateDirectory(_root);
		}


		public async Task<IList<ICandleF>> Get1mCandlesSpot(
			string symbol,
			CandleRange? req,
			Action<int, int>? tellProgress = null
		)
		{
			req ??= CandleRange.AllByNow();
			await BufferizeMissedCandlesIfNeed(symbol, req, tellProgress);
			return GetFromBuffer(symbol, req);
		}


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
				var candles = await _alternativeSource.Get1mCandlesSpot(symbol, range, tellProgress);
				Save(symbol, candles);
			}
		}


		private IList<ICandleF> GetFromBuffer(string symbol, CandleRange req)
		{
			var storage = SymbolToStorage(symbol);
			var candles = storage.Read(req);
			return candles;
		}


		private void Save(string symbol, IList<ICandleF> candlesToSave)
		{
			var storage = SymbolToStorage(symbol);
			storage.Save(candlesToSave);
			_safeRanges.UpdateRangeData(symbol, candlesToSave);
		}


		private FragmentaryCandleStorage SymbolToStorage(string symbol)
		{
			string storagePath = Path.Combine(_root, symbol);
			return new FragmentaryCandleStorage(storagePath);
		}
	}
}
