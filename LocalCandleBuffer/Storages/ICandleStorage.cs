using LocalCandleBuffer.Buffering.SymbolLevel;

namespace LocalCandleBuffer.Storages
{
	public interface ICandleStorage<TCandle>
		: IChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		/// <summary>
		/// Merges old and new candles and saves them all
		/// </summary>
		Task UpdateAndSave(Fragment<TCandle> newFragment);

		Task<DateRangeUtc?> GetLoadedRange();
	}
}
