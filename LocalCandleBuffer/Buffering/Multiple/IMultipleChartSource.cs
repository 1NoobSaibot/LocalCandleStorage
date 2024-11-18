namespace LocalCandleBuffer.Buffering.Multiple
{
	public interface IMultipleChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task<Fragment<TCandle>> Get1mCandles(
			string symbolId,
			DateRangeUtc req
		);
	}
}
