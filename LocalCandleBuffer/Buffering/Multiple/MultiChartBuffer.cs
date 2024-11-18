using LocalCandleBuffer.Buffering.Single;
using LocalCandleBuffer.Storages;

namespace LocalCandleBuffer.Buffering.Multiple
{
	public abstract class MultiChartBuffer<TCandle>
		: IMultipleChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _root;
		private readonly IMultipleChartSource<TCandle> _alternativeSource;


		public MultiChartBuffer(string root, IMultipleChartSource<TCandle> alternativeSource)
		{
			_root = root;
			_alternativeSource = alternativeSource;
			Directory.CreateDirectory(_root);
		}


		public async Task<Fragment<TCandle>> Get1mCandles(
			string symbol,
			DateRangeUtc req
		)
		{
			ISingleCandleSource<TCandle> singleChartBuffer = GetLocalBuffer(symbol);
			return await singleChartBuffer.Get1mCandles(req);
		}


		protected abstract ICandleStorage<TCandle> BuildStorage(string path);


		private ISingleCandleSource<TCandle> GetLocalBuffer(string symbol)
		{
			string storagePath = Path.Combine(_root, symbol);
			MultipleToSingleChartSourceAdapter<TCandle> wrapedRemoteSource = new(_alternativeSource, symbol);
			return new SingleChartBuffer<TCandle>(
				BuildStorage(storagePath),
				wrapedRemoteSource
			);
		}
	}
}
