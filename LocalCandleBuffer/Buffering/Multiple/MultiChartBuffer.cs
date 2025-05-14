using LocalCandleBuffer.Buffering.Single;
using LocalCandleBuffer.Helpers;
using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.Multiple
{
	public abstract class MultiChartBuffer<TCandle>
		: IMultipleChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		protected readonly string Root;
		private readonly IMultipleChartSource<TCandle> _alternativeSource;


		public MultiChartBuffer(string root, IMultipleChartSource<TCandle> alternativeSource)
		{
			Root = root;
			_alternativeSource = alternativeSource;
			Directory.CreateDirectory(Root);
		}


		public Task<Fragment<TCandle>> Get1mCandles(
			string symbol,
			DateRangeUtc req
		)
		{
			SingleChartBuffer<TCandle> singleChartBuffer = GetLocalBuffer(symbol);
			return singleChartBuffer.Get1mCandles(req);
		}


		public Task<Fragment<TCandle>> Get1mCandles(
			string symbol,
			DateRangeUtc req,
			Limit limit
		)
		{
			SingleChartBuffer<TCandle> singleChartBuffer = GetLocalBuffer(symbol);
			return singleChartBuffer.Get1mCandles(req, limit);
		}


		public async Task<string[]> GetAvailableSymbols()
		{
			string[] global = await _alternativeSource.GetAvailableSymbols();
			string[] local = GetAvailableSymbolsLocal();
			return ArrayEx.JoinUnique<string>(global, local);
		}


		protected abstract ICandleStorage<TCandle> BuildStorage(string path);
		public abstract string[] GetAvailableSymbolsLocal();

		private SingleChartBuffer<TCandle> GetLocalBuffer(string symbol)
		{
			string storagePath = Path.Combine(Root, symbol);
			MultipleToSingleChartSourceAdapter<TCandle> wrapedRemoteSource = new(_alternativeSource, symbol);
			return new SingleChartBuffer<TCandle>(
				BuildStorage(storagePath),
				wrapedRemoteSource
			);
		}
	}
}
