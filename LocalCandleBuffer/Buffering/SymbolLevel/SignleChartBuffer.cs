using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Types;

namespace LocalCandleBuffer.Buffering.SymbolLevel
{
	public class SingleChartBuffer<TCandle>
		: IChartSource<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly IChartSource<TCandle> _remouteSource;
		private readonly ICandleStorage<TCandle> _localStorage;
		public TimeFrame BaseTimeFrame => _remouteSource.BaseTimeFrame;


		public SingleChartBuffer(
			ICandleStorage<TCandle> localStorage,
			IChartSource<TCandle> alternativeSource
		)
		{
			_localStorage = localStorage;
			_remouteSource = alternativeSource;
		}


		public async Task<Fragment<TCandle>> GetCandles(DateRangeUtc req)
		{
			Fragment<TCandle> storedFrag = await _localStorage.GetCandles(req);
			if (storedFrag.IsEmpty)
			{
				Fragment<TCandle> loaded = await LoadAndSave(req);
				return loaded;
			}

			req.Validate(storedFrag);

			if (req.StartUTC < storedFrag.StartUtc)
			{
				DateRangeUtc tailReq = new(req.StartUTC, storedFrag.StartUtc);
				Fragment<TCandle> loadedTail = await LoadAndSave(tailReq, fromEndToStart: true);
				tailReq.Validate(loadedTail);
				storedFrag = storedFrag.Join(loadedTail);
			}
			if (req.EndUTC > storedFrag.EndUtc)
			{
				DateRangeUtc headReq = new(storedFrag.EndUtc, req.EndUTC);
				Fragment<TCandle> loadedHead = await LoadAndSave(headReq, fromEndToStart: false);
				headReq.Validate(loadedHead);
				storedFrag = storedFrag.Join(loadedHead);
			}

			return storedFrag;
		}


		public async Task<Fragment<TCandle>> GetCandles(DateRangeUtc req, Limit limit)
		{
			// TODO: Make the method more efficient
			var candles = await GetCandles(req);
			return candles.Pick(limit);
		}


		public Task<DateRangeUtc?> GetLoadedRange()
		{
			return _localStorage.GetLoadedRange();
		}


		private async Task<Fragment<TCandle>> LoadAndSave(DateRangeUtc range, bool fromEndToStart = true)
		{
			const int MAX_CHUNK_SIZE = 1440 * 30;

			Fragment<TCandle> res = Fragment<TCandle>.Empty(BaseTimeFrame);
			if (range.LengthIn1mCandles == 0)
			{
				return res;
			}

			Limit limit;
			if (fromEndToStart)
			{
				limit = Limit.FromTheEnd(MAX_CHUNK_SIZE);
			}
			else
			{
				limit = Limit.FromTheStart(MAX_CHUNK_SIZE);
			}

			DateRangeUtc remainRange = range;
			do
			{
				Fragment<TCandle> loaded = await _remouteSource
					.GetCandles(remainRange, limit);
				remainRange.Validate(loaded);
				res = res.Join(loaded);
				await _localStorage.UpdateAndSave(loaded);

				if (loaded.Count < MAX_CHUNK_SIZE)
				{
					// Probably we loaded all the candles we wanted.
					return res;
				}

				if (fromEndToStart)
				{
					if (loaded.StartUtc == range.StartUTC)
					{
						// We reached the start of the requested range.
						return res;
					}
					else
					{
						// Need to cut range from the end and keep loading.
						remainRange = new(remainRange.StartUTC, endUTC: loaded.StartUtc);
					}
				}
				else
				{
					if (loaded.EndUtc == range.EndUTC)
					{
						// We reached the end of the requested range.
						return res;
					}
					else
					{
						// Need to cut range from the start and keep loading.
						remainRange = new(startUTC: loaded.EndUtc, remainRange.EndUTC);
					}
				}
			}
			while (true);
		}
	}
}
