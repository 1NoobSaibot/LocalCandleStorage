using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer
{
	public class FragmentaryCandleStorage
	{
		private readonly string _root;


		public FragmentaryCandleStorage(string pathToFolder)
		{
			_root = pathToFolder;
			Directory.CreateDirectory(_root);
		}


		public IList<ICandleF> Read(CandleRange req)
		{
			int expectedCandles = req.GetLengthInMinutes();
			List<ICandleF> joined = new(expectedCandles);

			DateTime indexYear = req.StartUTC.RoundDownToYear();
			while (indexYear <= req.EndUTC)
			{
				LimitedCandleStorage fragStorage = DateToStorage(indexYear);
				IList<ICandleF> chunk = fragStorage.Read(req);
				joined.AddRange(chunk);
				indexYear = indexYear.AddYears(1);
			}

			return joined;
		}


		public void Save(IList<ICandleF> candles)
		{
			if (candles.Count == 0)
			{
				return;
			}

			DateTime indexYear = candles[0].OpenUtc.RoundDownToYear();
			do
			{
				LimitedCandleStorage yearStorage = DateToStorage(indexYear);
				yearStorage.Save(candles);

				indexYear = indexYear.AddYears(1);
			} while (indexYear <= candles.Last().OpenUtc);
		}


		private LimitedCandleStorage DateToStorage(DateTime date)
		{
			return new(
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
