namespace LocalCandleBuffer.Storages.Fragmented
{
	internal class Manifest<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _filePath;


		public Manifest(string filePath)
		{
			_filePath = filePath;
		}


		public DateRangeUtc? GetBufferedRangeData()
		{
			return Load();
		}


		public void UpdateRangeData(IList<TCandle> candles)
		{
			if (candles.Count == 0)
			{
				return;
			}
			UpdateRangeData(new DateRangeUtc(candles[0].OpenUtc, candles.Last().OpenUtc));
		}


		public void UpdateRangeData(DateRangeUtc newRange)
		{
			DateRangeUtc? oldRange = Load();
			if (oldRange is null)
			{
				Save(newRange);
				return;
			}

			if (oldRange.DoesTouch(newRange) == false)
			{
				Exception ex = new($"Ranges must touch each other: {oldRange} {newRange}");
				ex.Data.Add("Range1", oldRange);
				ex.Data.Add("Range2", newRange);
				throw ex;
			}

			DateRangeUtc extended = oldRange.Extend(newRange);
			if (extended.IsWiderThan(oldRange))
			{
				Save(extended);
			}
		}


		private DateRangeUtc? Load()
		{
			if (File.Exists(_filePath) == false)
			{
				return null;
			}

			using BinaryReader reader = new(new FileStream(_filePath, FileMode.Open));
			DateTime startUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
			DateTime endUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
			return new(startUtc, endUtc);
		}


		private void Save(DateRangeUtc range)
		{
			using BinaryWriter writer = new(new FileStream(_filePath, FileMode.Create));
			writer.Write(range.StartUTC.Ticks);
			writer.Write(range.EndUTC.Ticks);
			writer.Flush();
		}
	}
}
