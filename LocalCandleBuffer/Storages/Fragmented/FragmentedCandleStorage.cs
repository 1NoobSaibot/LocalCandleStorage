﻿using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer.Storages.Fragmented
{
	public abstract class FragmentedCandleStorage<TCandle>
		: ICandleStorage<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _root;
		private readonly Manifest<TCandle> _manifest;


		public FragmentedCandleStorage(string pathToFolder)
		{
			_root = pathToFolder;
			Directory.CreateDirectory(_root);
			_manifest = new(Path.Combine(pathToFolder, ManifestFileName()));
		}


		public async Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req)
		{
			Fragment<TCandle> res = new Fragment<TCandle>([], TimeFrame.OneMinute);

			DateTime indexYear = req.StartUTC.RoundDownToYear();
			while (indexYear <= req.EndUTC)
			{
				ICandleStorage<TCandle> fragStorage = DateToStorage(indexYear);
				Fragment<TCandle> chunk = await fragStorage.Get1mCandles(req);
				res = res.Join(chunk);
				indexYear = indexYear.AddYears(1);
			}

			return res;
		}


		public Task Save(Fragment<TCandle> candles)
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
				Fragment<TCandle> chunk = candles.Pick(new(indexYear, nextYear));
				yearStorage.Save(chunk);

				indexYear = nextYear;
				nextYear = nextYear.AddYears(1);
			} while (indexYear <= candles.Last().OpenUtc);

			_manifest.UpdateRangeData(candles.AsDateRange);

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