namespace LocalCandleBuffer
{
	public interface ICandleSource
	{
		Task<IList<ICandleF>> Get1mCandlesSpot(
			string symbolId,
			CandleRange? req = null,
			Action<int, int>? tellProgress = null
		);
	}
}
