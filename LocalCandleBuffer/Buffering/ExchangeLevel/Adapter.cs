using LocalCandleBuffer.Buffering.SymbolLevel;
using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.ExchangeLevel
{
	internal class Adapter<TCandle>
		: IChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly IExchangeCandleSource<TCandle> _originalSource;
		private readonly string _symbol;
		private readonly MarketType _marketType;
		public TimeFrame BaseTimeFrame => _originalSource.BaseTimeFrame;


		public Adapter(IExchangeCandleSource<TCandle> source, MarketType marketType, string symbol)
		{
			_originalSource = source;
			_symbol = symbol;
			_marketType = marketType;
		}


		public Task<Fragment<TCandle>> GetCandles(DateRangeUtc req)
		{
			return _originalSource.GetCandles(_marketType, _symbol, req);
		}


		public Task<Fragment<TCandle>> GetCandles(DateRangeUtc req, Limit limit)
		{
			return _originalSource.GetCandles(_marketType, _symbol, req, limit);
		}
	}
}
