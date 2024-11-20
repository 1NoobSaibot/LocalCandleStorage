using LocalCandleBuffer.Buffering.Single;

namespace LocalCandleBuffer.Storages
{
	public interface ICandleStorage<TCandle>
		: ISingleCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		/// <summary>
		/// Merges old and new candles and saves them all
		/// </summary>
		Task UpdateAndSave(Fragment<TCandle> newFragment);
	}
}
