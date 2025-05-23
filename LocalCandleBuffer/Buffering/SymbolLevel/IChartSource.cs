﻿using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.SymbolLevel
{
	public interface IChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		TimeFrame BaseTimeFrame { get; }


		Task<Fragment<TCandle>> GetCandles(
			DateRangeUtc req
		);


		Task<Fragment<TCandle>> GetCandles(
			DateRangeUtc req,
			Limit limit
		);
	}
}
