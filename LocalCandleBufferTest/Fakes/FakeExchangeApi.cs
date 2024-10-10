using LocalCandleBuffer;
using LocalCandleBufferTest.Implementations;

namespace LocalCandleBufferTest.Fakes
{
	internal class FakeExchangeApi : ICandleSource<Candle>, ICandleWritterReader<Candle>
	{
		private const string FILE_NAME = "CandleExample_2023.bin";
		private readonly BinaryCandleStorage<Candle> _storage;
		public static readonly FakeExchangeApi Instance = new FakeExchangeApi();
		public static readonly CandleRange AvailableRange = new(
			new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		);


		private FakeExchangeApi()
		{
			_storage = new(this, FILE_NAME);
		}


		public Task<IList<Candle>> Get1mCandlesSpot(
			string symbolId,
			CandleRange? req = null,
			Action<int, int>? tellProgress = null
		)
		{
			req ??= CandleRange.AllByNow();
			var candles = _storage.Read(req);
			return Task.FromResult(candles);
		}


		public IList<Candle> GetAllCandles()
		{
			return _storage.ReadAll();
		}

		public void WriteSingleCandleToFiler(Candle candle, BinaryWriter writer)
		{
			writer.Write(candle.Open);
			writer.Write(candle.High);
			writer.Write(candle.Low);
			writer.Write(candle.Close);
			writer.Write(candle.OpenUnixMc);
			writer.Write(candle.VolumeBase);
			writer.Write(candle.VolumeQuote);
		}


		public Candle ReadSingleCandleFromFile(BinaryReader reader)
		{
			return new Candle(
				open: reader.ReadSingle(),
				high: reader.ReadSingle(),
				low: reader.ReadSingle(),
				close: reader.ReadSingle(),
				openUnixMc: reader.ReadInt64(),
				volumeBase: reader.ReadSingle(),
				volumeQuote: reader.ReadSingle()
			);
		}
	}
}
