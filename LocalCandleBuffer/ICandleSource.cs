namespace LocalCandleBuffer
{
	public interface ICandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task<Fragment<TCandle>> Get1mCandles(
			string symbolId,
			CandleRange? req = null
		);
	}
}
