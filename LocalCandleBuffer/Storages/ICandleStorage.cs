using LocalCandleBuffer.Buffering.Single;

namespace LocalCandleBuffer.Storages
{
	public interface ICandleStorage<TCandle>
		: ISingleCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		Task Save(Fragment<TCandle> newFragment);
	}
}
