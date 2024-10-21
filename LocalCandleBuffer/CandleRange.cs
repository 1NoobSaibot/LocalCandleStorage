using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer
{
	// Describes the range of candles where the first candle OpenUtc is equal or greater than StartUTC
	// and the last candle OpenUtc is smaller or equal to EndUTC.
	// Range with single candle have StartUTC == EndUTC
	public class CandleRange
	{
		public readonly DateTime StartUTC;
		public readonly DateTime EndUTC;


		public CandleRange(DateTime startUTC, DateTime endUTC)
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


		public int GetLengthInMinutes()
		{
			return (int)(EndUTC - StartUTC).TotalMinutes;
		}


		public bool InRange<TCandle>(TCandle candle) where TCandle : IStorableCandle<TCandle>
		{
			return candle.OpenUtc >= StartUTC && candle.OpenUtc < EndUTC;
		}


		public bool DoesTouch(CandleRange anotherRange)
		{
			return this.EndUTC >= anotherRange.StartUTC
				|| anotherRange.EndUTC >= this.StartUTC;
		}


		public IList<CandleRange> ToDescendingChunks(TimeSpan chunkSize)
		{
			DateTime localEnd = EndUTC;
			DateTime localStart = localEnd - chunkSize;
			List<CandleRange> frags = [];

			while (localStart >= StartUTC)
			{
				frags.Add(new(localStart, localEnd));
				localEnd = localStart;
				localStart = localEnd - chunkSize;
			}

			if (localEnd > StartUTC || frags.Count == 0)
			{
				frags.Add(new(StartUTC, localEnd));
			}

			return frags;
		}


		public IList<CandleRange> ToAscendingChunks(TimeSpan chunkSize)
		{
			DateTime localStart = StartUTC;
			DateTime localEnd = localStart + chunkSize;
			List<CandleRange> frags = [];

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

		public static CandleRange AllByNow()
		{
			DateTime utcMinValue = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return new(
				utcMinValue,
				DateTime.UtcNow.RoundDownToMinutes()
			);
		}
	}
}
