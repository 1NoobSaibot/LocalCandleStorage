using LocalCandleBuffer.Buffering.Single;

namespace LocalCandleBuffer.Buffering.Multiple
{
	internal class MultipleToSingleChartSourceAdapter<TCandle>
		: ISingleCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly IMultipleChartSource<TCandle> _originalSource;
		private readonly string _symbol;


		public MultipleToSingleChartSourceAdapter(IMultipleChartSource<TCandle> source, string symbol)
		{
			_originalSource = source;
			_symbol = symbol;
		}


		public Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req)
		{
			return _originalSource.Get1mCandles(_symbol, req);
		}
	}
}
