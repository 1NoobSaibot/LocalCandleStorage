using SafeFile.Atomic;

namespace LocalCandleBuffer.Storages.Fragmented
{
	internal class Manifest<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly AtomicFileStorage _storage;


		public Manifest(string filePath)
		{
			_storage = new(filePath);
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
			_storage.TryRead<DateRangeUtc>(
				reader =>
				{
					DateTime startUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
					DateTime endUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
					return new(startUtc, endUtc);
				},
				out DateRangeUtc? range
			);
			return range;
		}


		private void Save(DateRangeUtc range)
		{
			_storage.WriteAndSave(writer =>
			{
				writer.Write(range.StartUTC.Ticks);
				writer.Write(range.EndUTC.Ticks);
			});
		}
	}
}
