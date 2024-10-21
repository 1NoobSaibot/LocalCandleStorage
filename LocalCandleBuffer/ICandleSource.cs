namespace LocalCandleBuffer
{
	public interface ICandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task<IList<TCandle>> Get1mCandles(
			string symbolId,
			CandleRange? req = null,
			Action<int, int>? tellProgress = null
		);
	}
}
