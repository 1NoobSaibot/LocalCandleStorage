using LocalCandleBuffer.Storages;

namespace LocalCandleBuffer.Buffering.Single
{
	public class SingleChartBuffer<TCandle>
		: ISingleCandleSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly ISingleCandleSource<TCandle> _remouteSource;
		private readonly ICandleStorage<TCandle> _localStorage;


		public SingleChartBuffer(
			ICandleStorage<TCandle> localStorage,
			ISingleCandleSource<TCandle> alternativeSource
		)
		{
			_localStorage = localStorage;
			_remouteSource = alternativeSource;
		}


		public async Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req)
		{
			Fragment<TCandle> storedFrag = await _localStorage.Get1mCandles(req);
			if (storedFrag.IsEmpty)
			{
				Fragment<TCandle> loaded = await _remouteSource.Get1mCandles(req);
				req.Validate(loaded);
				await _localStorage.UpdateAndSave(loaded);
				return loaded;
			}

			req.Validate(storedFrag);

			bool needToSave = false;
			if (req.StartUTC < storedFrag.StartUtc)
			{
				DateRangeUtc tailReq = new(req.StartUTC, storedFrag.StartUtc);
				Fragment<TCandle> loadedTail = await _remouteSource.Get1mCandles(tailReq);
				tailReq.Validate(loadedTail);
				storedFrag = storedFrag.Join(loadedTail);
				needToSave = true;
			}
			if (req.EndUTC > storedFrag.EndUtc)
			{
				DateRangeUtc headReq = new(storedFrag.EndUtc, req.EndUTC);
				Fragment<TCandle> loadedHead = await _remouteSource.Get1mCandles(headReq);
				headReq.Validate(loadedHead);
				storedFrag = storedFrag.Join(loadedHead);
				needToSave = true;
			}

			if (needToSave)
			{
				await _localStorage.UpdateAndSave(storedFrag);
			}

			return storedFrag;
		}
	}
}
