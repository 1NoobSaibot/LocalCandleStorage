using LocalCandleBuffer;

namespace LocalCandleBufferTest.Implementations
{
	internal class Candle : IStorableCandle<Candle>
	{
		public readonly float Open;
		public readonly float High;
		public readonly float Low;
		public readonly float Close;
		public readonly float VolumeBase;
		public readonly float VolumeQuote;

		public DateTime OpenUtc { get; }
		public long OpenUnixMc { get; }


		public Candle(
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
			VolumeBase = volumeBase;
			VolumeQuote = volumeQuote;
			OpenUnixMc = openUnixMc;
			OpenUtc = DateTimeOffset.FromUnixTimeMilliseconds(OpenUnixMc)
				.UtcDateTime;
		}


		public Candle(
			float open,
			float high,
			float low,
			float close,
			DateTime openUtc,
			float volumeBase,
			float volumeQuote
		)
		{
			if (openUtc.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException($"Kind of {nameof(openUtc)} mus tbe UTC");
			}

			Open = open;
			High = high;
			Low = low;
			Close = close;
			VolumeBase = volumeBase;
			VolumeQuote = volumeQuote;
			OpenUnixMc = new DateTimeOffset(openUtc, TimeSpan.Zero)
				.ToUnixTimeMilliseconds();
			OpenUtc = openUtc;
		}


		public bool IsThisCandleNewerThan(Candle other)
		{
			return VolumeBase > other.VolumeBase;
		}


		public Candle MergeWith(Candle other)
		{
			return new Candle(
				open: this.Open,
				high: Math.Max(this.High, other.High),
				low: Math.Min(this.Low, other.Low),
				close: other.Close,
				openUnixMc: this.OpenUnixMc,
				volumeBase: this.VolumeBase + other.VolumeBase,
				volumeQuote: this.VolumeQuote + other.VolumeQuote
			);
		}


		public Candle ShiftOpenUtc(DateTime newOpenUtc)
		{
			return new Candle(
				open: this.Open,
				high: this.High,
				low: this.Low,
				close: this.Close,
				openUtc: newOpenUtc,
				volumeBase: this.VolumeBase,
				volumeQuote: this.VolumeQuote
			);
		}
	}
}
