using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer
{
	// Describes the range of candles where the first candle OpenUtc is equal or greater than StartUTC
	// and the last candle OpenUtc is smaller or equal to EndUTC.
	// Range with single candle have StartUTC == EndUTC
	public class DateRangeUtc
	{
		public readonly DateTime StartUTC;
		public readonly DateTime EndUTC;

		private static readonly DateTime MIN_UTC_VALUE = new(0, DateTimeKind.Utc);


		public DateRangeUtc(DateTime startUTC, DateTime endUTC)
		{
			if (startUTC.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("Kind of StartUTC must be UTC");
			}
			if (endUTC.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("Kind of EndUTC must be UTC");
			}
			if (startUTC.IsRoundedToMinutes() == false)
			{
				throw new ArgumentException("StartUTC must be rounded to minutes");
			}
			if (endUTC.IsRoundedToMinutes() == false)
			{
				throw new ArgumentException("EndUTC must be rounded to minutes");
			}

			if (startUTC > endUTC)
			{
				throw new ArgumentException("StartUTC must be greater than or equal to EndUTC");
			}

			StartUTC = startUTC;
			EndUTC = endUTC;
		}


		public bool InRange<TCandle>(TCandle candle) where TCandle : IStorableCandle<TCandle>
		{
			return candle.OpenUtc >= StartUTC && candle.OpenUtc < EndUTC;
		}


		public bool DoesTouch(DateRangeUtc anotherRange)
		{
			return this.StartUTC <= anotherRange.EndUTC
				&& anotherRange.StartUTC <= this.EndUTC;
		}


		public IList<DateRangeUtc> ToAscendingChunks(TimeSpan chunkSize)
		{
			DateTime localStart = StartUTC;
			DateTime localEnd = localStart + chunkSize;
			List<DateRangeUtc> frags = [];

			while (localEnd <= EndUTC)
			{
				frags.Add(new(localStart, localEnd));
				localStart = localEnd;
				localEnd = localStart + chunkSize;
			}

			if (localStart < EndUTC || frags.Count == 0)
			{
				frags.Add(new(localStart, EndUTC));
			}

			return frags;
		}

		public static DateRangeUtc AllByNow()
		{
			DateTime utcMinValue = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return new(
				utcMinValue,
				DateTime.UtcNow.RoundDownToMinutes()
			);
		}


		public DateRangeUtc Extend(DateRangeUtc another)
		{
			DateTime extendedStart = this.StartUTC < another.StartUTC
				? this.StartUTC
				: another.StartUTC;
			DateTime extendedEnd = this.EndUTC > another.EndUTC
				? this.EndUTC
				: another.EndUTC;

			return new(extendedStart, extendedEnd);
		}


		public bool IsWiderThan(DateRangeUtc another)
		{
			return (this.StartUTC < another.StartUTC && this.EndUTC >= another.EndUTC)
				|| (this.StartUTC <= another.StartUTC && this.EndUTC > another.EndUTC);
		}


		public static DateRangeUtc All(TimeFrame tf)
		{
			long maxTicks = DateTime.MaxValue.Ticks;
			DateTime maxUtc = new(maxTicks - (maxTicks % tf.AsTimeSpan.Ticks), DateTimeKind.Utc);
			return new(MIN_UTC_VALUE, maxUtc);
		}


		public static DateRangeUtc Until(DateTime endDateUtc)
		{
			return new DateRangeUtc(MIN_UTC_VALUE, endDateUtc);
		}


		public void Validate<TCandle>(Fragment<TCandle> frag) where TCandle : IStorableCandle<TCandle>
		{
			if (frag.StartUtc < StartUTC)
			{
				throw new ArgumentException($"The fragment doesn't match {nameof(StartUTC)}-param.");
			}
			if (frag.EndUtc > EndUTC)
			{
				throw new ArgumentException($"The fragment doesn't match {nameof(EndUTC)}-param.");
			}
		}
	}
}
