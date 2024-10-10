using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer
{
	internal class FragmentaryCandleStorage<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _root;
		private readonly ICandleWritterReader<TCandle> _readerWriter;


		public FragmentaryCandleStorage(
			ICandleWritterReader<TCandle> readerWriter,
			string pathToFolder)
		{
			_root = pathToFolder;
			_readerWriter = readerWriter;
			Directory.CreateDirectory(_root);
		}


		public IList<TCandle> Read(CandleRange req)
		{
			int expectedCandles = req.GetLengthInMinutes();
			List<TCandle> joined = new(expectedCandles);

			DateTime indexYear = req.StartUTC.RoundDownToYear();
			while (indexYear <= req.EndUTC)
			{
				LimitedCandleStorage<TCandle> fragStorage = DateToStorage(indexYear);
				IList<TCandle> chunk = fragStorage.Read(req);
				joined.AddRange(chunk);
				indexYear = indexYear.AddYears(1);
			}

			return joined;
		}


		public void Save(IList<TCandle> candles)
		{
			if (candles.Count == 0)
			{
				return;
			}

			DateTime indexYear = candles[0].OpenUtc.RoundDownToYear();
			do
			{
				LimitedCandleStorage<TCandle> yearStorage = DateToStorage(indexYear);
				yearStorage.Save(candles);

				indexYear = indexYear.AddYears(1);
			} while (indexYear <= candles.Last().OpenUtc);
		}


		private LimitedCandleStorage<TCandle> DateToStorage(DateTime date)
		{
			return new(
				_readerWriter,
				DateToFileName(date),
				date,
				date.AddYears(1)
			);
		}


		private string DateToFileName(DateTime date)
		{
			return Path.Combine(_root, date.Year.ToString()) + ".bin";
		}
	}
}
