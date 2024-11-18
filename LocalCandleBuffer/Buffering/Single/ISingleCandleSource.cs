namespace LocalCandleBuffer.Buffering.Single
{
	public interface ISingleCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req);
	}
}
