using LocalCandleBuffer;
using LocalCandleBuffer.Buffering.Multiple;
using LocalCandleBuffer.Storages;
using LocalCandleBufferTest.Implementations;

namespace LocalCandleBufferTest.Fakes
{
	internal class FakeExchangeApi : IMultipleChartSource<Candle>
	{
		private const string FILE_NAME = "../../../../CandleExample_2023.bin";
		private readonly BinaryCandleStorage<Candle> _storage;
		public static readonly FakeExchangeApi Instance = new FakeExchangeApi();
		public static readonly DateRangeUtc AvailableRange = new(
			new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		);


		private FakeExchangeApi()
		{
			_storage = new FileStorage(FILE_NAME);
		}


		public async Task<Fragment<Candle>> Get1mCandles(
			string symbolId,
			DateRangeUtc? req = null
		)
		{
			req ??= DateRangeUtc.AllByNow();
			var candles = await _storage.Get1mCandles(req);
			Fragment<Candle> frag = new(candles.ToArray(), TimeFrame.OneMinute);
			return frag;
		}


		public Task<Fragment<Candle>> GetAllCandles()
		{
			return _storage.Get1mCandles(DateRangeUtc.All(TimeFrame.OneMinute));
		}


		public Task<Fragment<Candle>> GetAll1mCandlesBefore(string symbolId, DateTime endDateUtc)
		{
			throw new NotImplementedException();
		}
	}
}
