using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.Multiple
{
	public interface IMultipleChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task<Fragment<TCandle>> Get1mCandles(
			string symbolId,
			DateRangeUtc req
		);


		Task<Fragment<TCandle>> Get1mCandles(
			string symbolId,
			DateRangeUtc req,
			Limit limit
		);
	}
}
