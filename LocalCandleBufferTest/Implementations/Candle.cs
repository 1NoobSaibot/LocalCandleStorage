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
			OpenUtc = DateTimeOffset.FromUnixTimeMilliseconds(OpenUnixMc).UtcDateTime;
		}

		public bool IsThisVolumeBiggerThan(Candle other)
		{
			return VolumeBase > other.VolumeBase;
		}
	}
}
