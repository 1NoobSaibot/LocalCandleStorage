using LocalCandleBuffer.Helpers;
using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Storages.Fragmented
{
	public abstract class FragmentedCandleStorage<TCandle>
		: ICandleStorage<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _root;
		private readonly Manifest<TCandle> _manifest;
		public TimeFrame BaseTimeFrame { get; }


		public FragmentedCandleStorage(string pathToFolder, TimeFrame baseTimeFrame)
		{
			_root = pathToFolder;
			BaseTimeFrame = baseTimeFrame;
			Directory.CreateDirectory(_root);
			_manifest = new(Path.Combine(pathToFolder, ManifestFileName()));
			BaseTimeFrame = baseTimeFrame;
		}


		public Task<DateRangeUtc?> GetLoadedRange()
		{
			return Task.FromResult(_manifest.GetBufferedRangeData());
		}


		public async Task<Fragment<TCandle>> GetCandles(DateRangeUtc req)
		{
			Fragment<TCandle> res = Fragment<TCandle>.Empty(BaseTimeFrame);

			DateRangeUtc? bufferedRange = _manifest.GetBufferedRangeData();
			if (bufferedRange is null)
			{
				return res;
			}

			DateRangeUtc? commonRange = bufferedRange.GetCommonOrNull(req);
			if (commonRange is null)
			{
				return res;
			}

			DateTime indexYear = commonRange.StartUTC.RoundDownToYear();
			while (indexYear <= commonRange.EndUTC)
			{
				ICandleStorage<TCandle> fragStorage = DateToStorage(indexYear);
				Fragment<TCandle> chunk = await fragStorage.GetCandles(commonRange);
				res = res.Join(chunk);
				indexYear = indexYear.AddYears(1);
			}

			return res;
		}


		public async Task<Fragment<TCandle>> GetCandles(DateRangeUtc req, Limit limit)
		{
			// TODO: Make the method more efficient
			var candles = await GetCandles(req);
			return candles.Pick(limit);
		}


		public Task UpdateAndSave(Fragment<TCandle> candles)
		{
			if (candles.IsEmpty)
			{
				return Task.CompletedTask;
			}

			DateTime indexYear = candles[0].OpenUtc.RoundDownToYear();
			DateTime nextYear = indexYear.AddYears(1);
			do
			{
				ICandleStorage<TCandle> yearStorage = DateToStorage(indexYear);
				Fragment<TCandle> chunk = candles.Pick(new DateRangeUtc(indexYear, nextYear));
				yearStorage.UpdateAndSave(chunk);

				indexYear = nextYear;
				nextYear = nextYear.AddYears(1);
			} while (indexYear <= candles.Last().OpenUtc);

			_manifest.UpdateRangeData(candles.AsDateRange!);

			return Task.CompletedTask;
		}


		protected virtual string ManifestFileName()
		{
			return "manifest.bin";
		}


		protected abstract ICandleStorage<TCandle> CreateStorage(string path);


		private ICandleStorage<TCandle> DateToStorage(DateTime date)
		{
			string path = DateToFileName(date);
			return CreateStorage(path);
		}


		private string DateToFileName(DateTime date)
		{
			return Path.Combine(_root, date.Year.ToString()) + ".bin";
		}
	}
}
