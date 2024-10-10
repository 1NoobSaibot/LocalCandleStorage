namespace LocalCandleBuffer
{
	public interface ICandleF
	{
		float Open { get; }
		float High { get; }
		float Low { get; }
		float Close { get; }
		long OpenUnixMc { get; }
		DateTime OpenUtc { get; }
		float VolumeBase { get; }
		float VolumeQuote { get; }


		// TODO: you can check Volumes now
		public bool IsNewerThan(ICandleF anotherCandle)
		{
			if (anotherCandle.Open != this.Open || anotherCandle.OpenUtc != this.OpenUtc)
			{
				throw new Exception("Open prices or dates are different, something went wrong");
			}

			// If our high-low range is wider then we are newer
			if (this.High > anotherCandle.High)
			{
				if (this.Low > anotherCandle.Low)
				{
					throw new Exception("Ranges are intersected. Cannot say who is older or newer");
				}
				return true;
			}

			if (this.Low < anotherCandle.Low)
			{
				if (this.High < anotherCandle.High)
				{
					throw new Exception("Ranges are intersected. Cannot say who is older or newer");
				}
				return true;
			}

			// Our range is not wider,
			// so the only way we can be newer is to have equivalent range and different close price
			return this.High == anotherCandle.High
				&& this.Low == anotherCandle.Low
				&& this.Close != anotherCandle.Close;
		}
	}

	public class CandleF : ICandleF
	{
		public float Open { get; }
		public float High { get; }
		public float Low { get; }
		public float Close { get; }
		public long OpenUnixMc { get; }
		public DateTime OpenUtc { get; }
		public float VolumeBase { get; }
		public float VolumeQuote { get; }


		public CandleF(
			float open,
			float high,
			float low,
			float close,
			long openUnixMc,
			float volumeBase,
			float volumeQuote
		)
		{
			Open = open;
			High = high;
			Low = low;
			Close = close;
			OpenUnixMc = openUnixMc;
			OpenUtc = DateTimeOffset
				.FromUnixTimeMilliseconds(openUnixMc).DateTime;
			VolumeBase = volumeBase;
			VolumeQuote = volumeQuote;
		}


		public CandleF(
			float open,
			float high,
			float low,
			float close,
			DateTime openUtc,
			float volumeBase,
			float volumeQuote
		)
		{
			Open = open;
			High = high;
			Low = low;
			Close = close;
			OpenUnixMc = new DateTimeOffset(openUtc).ToUnixTimeMilliseconds();
			OpenUtc = openUtc;
			VolumeBase = volumeBase;
			VolumeQuote = volumeQuote;
		}
	}
}
