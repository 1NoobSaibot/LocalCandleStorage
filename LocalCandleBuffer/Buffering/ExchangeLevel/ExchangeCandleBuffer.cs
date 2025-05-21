using LocalCandleBuffer.Buffering.SymbolLevel;
using LocalCandleBuffer.Helpers;
using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.ExchangeLevel
{
	public abstract class ExchangeCandleBuffer<TCandle>
		: IExchangeCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		protected readonly string Root;
		private readonly IExchangeCandleSource<TCandle> _alternativeSource;
		public MarketType[] AvailableMarketTypes => _alternativeSource.AvailableMarketTypes;
		public TimeFrame BaseTimeFrame => _alternativeSource.BaseTimeFrame;


		public ExchangeCandleBuffer(
			string root,
			IExchangeCandleSource<TCandle> alternativeSource
		)
		{
			Root = root;
			_alternativeSource = alternativeSource;
			Directory.CreateDirectory(Root);
		}


		public Task<Fragment<TCandle>> GetCandles(
			MarketType marketType,
			string symbol,
			DateRangeUtc req
		)
		{
			SingleChartBuffer<TCandle> singleChartBuffer = GetSymbolBuffer(marketType, symbol);
			return singleChartBuffer.GetCandles(req);
		}


		public Task<Fragment<TCandle>> GetCandles(
			MarketType marketType,
			string symbol,
			DateRangeUtc req,
			Limit limit
		)
		{
			SingleChartBuffer<TCandle> singleChartBuffer = GetSymbolBuffer(marketType, symbol);
			return singleChartBuffer.GetCandles(req, limit);
		}


		public async Task<string[]> GetAvailableSymbols(MarketType marketType)
		{
			string[] global = await _alternativeSource.GetAvailableSymbols(marketType);
			string[] local = GetAvailableSymbolsLocal(marketType);
			return ArrayEx.JoinUnique(global, local);
		}


		public Task<DateRangeUtc?> GetLoadedRange(MarketType marketType, string symbolId)
		{
			SingleChartBuffer<TCandle> singleChartBuffer = GetSymbolBuffer(marketType, symbolId);
			return singleChartBuffer.GetLoadedRange();
		}



		protected abstract ICandleStorage<TCandle> BuildStorage(string path);
		public abstract string[] GetAvailableSymbolsLocal(MarketType marketType);

		private SingleChartBuffer<TCandle> GetSymbolBuffer(MarketType marketType, string symbolId)
		{
			string storagePath = Path.Combine(Root, marketType.ToString(), symbolId);
			Adapter<TCandle> wrapedRemoteSource = new(_alternativeSource, marketType, symbolId);
			return new SingleChartBuffer<TCandle>(
				BuildStorage(storagePath),
				wrapedRemoteSource
			);
		}
	}
}
