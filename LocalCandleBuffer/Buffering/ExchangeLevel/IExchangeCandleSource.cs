using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.ExchangeLevel
{
	public interface IExchangeCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		MarketType[] AvailableMarketTypes { get; }
		TimeFrame BaseTimeFrame { get; }

		Task<string[]> GetAvailableSymbols(
			MarketType marketType
		);

		Task<Fragment<TCandle>> GetCandles(
			MarketType marketType,
			string symbolId,
			DateRangeUtc req
		);


		Task<Fragment<TCandle>> GetCandles(
			MarketType marketType,
			string symbolId,
			DateRangeUtc req,
			Limit limit
		);
	}
}
