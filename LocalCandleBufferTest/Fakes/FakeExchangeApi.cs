using LocalCandleBuffer;
using LocalCandleBuffer.Buffering.ExchangeLevel;
using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Types;
using LocalCandleBufferTest.Implementations;

namespace LocalCandleBufferTest.Fakes
{
	internal class FakeExchangeApi : IExchangeCandleSource<Candle>
	{
		private const string FILE_NAME = "../../../../CandleExample_2023.bin";
		private readonly BinaryCandleStorage<Candle> _storage;
		public static readonly FakeExchangeApi Instance = new();
		public static readonly DateRangeUtc AvailableRange = new(
			new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		);
		public TimeFrame BaseTimeFrame => _storage.BaseTimeFrame;
		public MarketType[] AvailableMarketTypes => [MarketType.Spot];


		private FakeExchangeApi()
		{
			_storage = new FileStorage(FILE_NAME, TimeFrame.OneMinute);
		}


		public async Task<Fragment<Candle>> GetCandles(
			MarketType marketType,
			string symbolId,
			DateRangeUtc? req = null
		)
		{
			req ??= DateRangeUtc.AllByNow();
			var candles = await _storage.GetCandles(req);
			Fragment<Candle> frag = new([.. candles], TimeFrame.OneMinute);
			return frag;
		}


		public Task<Fragment<Candle>> GetCandles(
			MarketType marketType,
			string symbolId,
			DateRangeUtc req,
			Limit limit
		)
		{
			throw new NotImplementedException();
		}


		public Task<Fragment<Candle>> GetAllCandles()
		{
			return _storage.GetCandles(DateRangeUtc.All(TimeFrame.OneMinute));
		}


		public Task<string[]> GetAvailableSymbols(MarketType marketType)
		{
			throw new NotImplementedException();
		}
	}
}
