﻿using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Types;

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
				Fragment<TCandle> loaded = await LoadAndSave(req);
				return loaded;
			}

			req.Validate(storedFrag);

			if (req.StartUTC < storedFrag.StartUtc)
			{
				DateRangeUtc tailReq = new(req.StartUTC, storedFrag.StartUtc);
				Fragment<TCandle> loadedTail = await LoadAndSave(tailReq);
				tailReq.Validate(loadedTail);
				storedFrag = storedFrag.Join(loadedTail);
			}
			if (req.EndUTC > storedFrag.EndUtc)
			{
				DateRangeUtc headReq = new(storedFrag.EndUtc, req.EndUTC);
				Fragment<TCandle> loadedHead = await LoadAndSave(headReq);
				headReq.Validate(loadedHead);
				storedFrag = storedFrag.Join(loadedHead);
			}

			return storedFrag;
		}


		public Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req, Limit limit)
		{
			throw new NotImplementedException();
		}


		private async Task<Fragment<TCandle>> LoadAndSave(DateRangeUtc range)
		{
			const int MAX_CHUNK_SIZE = 1440 * 30;

			Fragment<TCandle> res = Fragment<TCandle>.Empty(TimeFrame.OneMinute);
			if (range.LengthIn1mCandles == 0)
			{
				return res;
			}

			Limit limit = Limit.FromTheEnd(MAX_CHUNK_SIZE);
			DateRangeUtc remainRange = range;
			do
			{
				Fragment<TCandle> loaded = await _remouteSource
					.Get1mCandles(remainRange, limit);
				remainRange.Validate(loaded);
				res = res.Join(loaded);
				await _localStorage.UpdateAndSave(loaded);

				if (
					loaded.Count < MAX_CHUNK_SIZE
					|| loaded.StartUtc == range.StartUTC
				)
				{
					/*
						if we've got less candles than we asked then there is no more candles so stop loading.
						if the start of the fragment is equal to the start of requested range,
						then we loaded all we requested.
					*/
					return res;
				}

				remainRange = new(remainRange.StartUTC, endUTC: loaded.StartUtc);
			}
			while (true);
		}
	}
}
