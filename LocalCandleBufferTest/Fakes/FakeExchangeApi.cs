using LocalCandleBuffer;

namespace LocalCandleBufferTest.Fakes
{
	internal class FakeExchangeApi : ICandleSource
	{
		private const string FILE_NAME = "CandleExample_2023.bin";
		private readonly BinaryCandleStorage _storage = new(FILE_NAME);
		public static readonly FakeExchangeApi Instance = new FakeExchangeApi();
		public static readonly CandleRange AvailableRange = new(
			new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		);


		private FakeExchangeApi() { }


		public Task<IList<ICandleF>> Get1mCandlesSpot(
			string symbolId,
			CandleRange? req = null,
			Action<int, int>? tellProgress = null
		)
		{
			req ??= CandleRange.AllByNow();
			var candles = _storage.Read(req);
			return Task.FromResult(candles);
		}


		public IList<ICandleF> GetAllCandles()
		{
			return _storage.ReadAll();
		}
	}
}
