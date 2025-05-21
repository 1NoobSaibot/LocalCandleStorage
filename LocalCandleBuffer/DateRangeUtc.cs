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
		public TimeSpan TimeSpan => EndUTC - StartUTC;
		public int LengthIn1mCandles => (int)TimeSpan.TotalMinutes;

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


		public static DateRangeUtc AllByNow()
		{
			DateTime utcMinValue = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
			DateTime maxUtc = GetMaxUtcValueForTimeFrame(tf);
			return new(MIN_UTC_VALUE, maxUtc);
		}


		private static DateTime GetMaxUtcValueForTimeFrame(TimeFrame timeFrame)
		{
			long maxTicks = DateTime.MaxValue.Ticks;
			DateTime maxUtc = new(maxTicks - (maxTicks % timeFrame.AsTimeSpan.Ticks), DateTimeKind.Utc);
			return maxUtc;
		}


		public static DateRangeUtc Until(DateRangeUtc range)
		{
			return Until(endDateUtc: range.StartUTC);
		}


		public static DateRangeUtc Until(DateTime endDateUtc)
		{
			return new DateRangeUtc(MIN_UTC_VALUE, endDateUtc);
		}


		public static DateRangeUtc After(DateRangeUtc range, TimeFrame targetTimeFrame)
		{
			return After(startDateUtc: range.EndUTC, targetTimeFrame);
		}


		public static DateRangeUtc After(DateTime startDateUtc, TimeFrame targetTimeFrame)
		{
			DateTime maxUtc = GetMaxUtcValueForTimeFrame(targetTimeFrame);
			return new DateRangeUtc(startUTC: startDateUtc, endUTC: maxUtc);
		}


		public void Validate<TCandle>(Fragment<TCandle> frag) where TCandle : IStorableCandle<TCandle>
		{
			if (frag.IsEmpty)
			{
				// Empty fragment is always valid.
				return;
			}

			if (frag.StartUtc < StartUTC)
			{
				throw new ArgumentException($"The fragment doesn't match {nameof(StartUTC)}-param.");
			}
			if (frag.EndUtc > EndUTC)
			{
				throw new ArgumentException($"The fragment doesn't match {nameof(EndUTC)}-param.");
			}
		}


		public DateRangeUtc? GetCommonOrNull(DateRangeUtc another)
		{
			DateTime startUTC = this.StartUTC > another.StartUTC ? this.StartUTC : another.StartUTC;
			DateTime endUTC = this.EndUTC < another.EndUTC ? this.EndUTC : another.EndUTC;
			if (startUTC > endUTC)
			{
				return null;
			}

			return new DateRangeUtc(
				startUTC: startUTC,
				endUTC: endUTC
			);
		}


		public DateRangeUtc[] SplitBy(DateTime momentUtc)
		{
			if (momentUtc <= StartUTC || momentUtc >= EndUTC)
			{
				return [this];
			}

			return [
				new(StartUTC, momentUtc),
				new(momentUtc, EndUTC)
			];
		}


		public override string ToString()
		{
			return $"[{StartUTC} - {EndUTC}]";
		}


		public void WriteToBinaryWriter(BinaryWriter writer)
		{
			writer.Write(new DateTimeOffset(StartUTC).ToUnixTimeMilliseconds());
			writer.Write(new DateTimeOffset(EndUTC).ToUnixTimeMilliseconds());
		}


		public static DateRangeUtc ReadFromBinaryReader(BinaryReader reader)
		{
			return new DateRangeUtc(
				startUTC: DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()).UtcDateTime,
				endUTC: DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()).UtcDateTime
			);
		}
	}
}
