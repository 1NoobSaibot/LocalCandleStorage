using System.Collections;

namespace LocalCandleBuffer
{
	public class Fragment<TCandle> : IEnumerable<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly TCandle[] _m;
		public readonly TimeFrame TimeFrame;

		public bool IsEmpty => _m.Length == 0;
		public int Count => _m.Length;
		public DateTime StartUtc => IsEmpty
			? throw new Exception($"Empty Fragment can't have a {nameof(StartUtc)} date")
			: _m[0].OpenUtc;
		public DateTime EndUtc => IsEmpty
			? throw new Exception($"Empty Fragment can't have an {nameof(EndUtc)} date")
			: (_m[^1].OpenUtc + TimeFrame.AsTimeSpan);
		public DateRangeUtc AsDateRange => new(StartUtc, EndUtc);


		public Fragment(TCandle[] candles, TimeFrame tf)
		{
			_m = candles;
			TimeFrame = tf;

			if (candles.Length == 0)
			{
				return;
			}

			ValidateOpenDate(candles[0].OpenUtc);
			DateTime prev = candles[0].OpenUtc;

			for (int i = 1; i < candles.Length; i++)
			{
				ValidateOpenDate(candles[i].OpenUtc);
				TimeSpan diffFromPrev = candles[i].OpenUtc - prev;
				if (diffFromPrev <= TimeSpan.Zero)
				{
					throw new ArgumentException("Candles are not sorted in ascending order by OpenUtc date");
				}
				prev = candles[i].OpenUtc;
			}

			void ValidateOpenDate(DateTime open)
			{
				if (open.Kind != DateTimeKind.Utc)
				{
					throw new ArgumentException("DateTime.Kind must be UTC");
				}
				if (open.Ticks % tf.AsTimeSpan.Ticks != 0)
				{
					throw new ArgumentException("DateTime doesn't match TimeFrame");
				}
			}
		}


		public TCandle[] GetCandles()
		{
			TCandle[] res = new TCandle[_m.Length];
			Array.Copy(_m, res, _m.Length);
			return res;
		}


		public TCandle Last()
		{
			return _m.Last();
		}


		/// <summary>
		/// NOTE: It doesn't change existing fragments, creates a new fragment instead.
		/// Must Use Returned Value
		/// </summary>
		/// <param name="anotherFrag"></param>
		/// <returns>A fragment which contains all candles from both input fragments</returns>
		/// <exception cref="ArgumentException"></exception>
		public Fragment<TCandle> Join(Fragment<TCandle> anotherFrag)
		{
			if (this.TimeFrame != anotherFrag.TimeFrame)
			{
				throw new ArgumentException("Fragments must have the same TimeFrame");
			}

			if (this.IsEmpty)
			{
				return anotherFrag;
			}
			if (anotherFrag.IsEmpty)
			{
				return this;
			}

			// TODO: Gluing is not such efficient when fragments has no intersections
			return new Fragment<TCandle>(
				[.. SimpleCandleRepair.Glue(this._m, anotherFrag._m)],
				TimeFrame
			);
		}


		public IEnumerator<TCandle> GetEnumerator()
		{
			foreach (var item in _m)
			{
				yield return (TCandle)item;
			}
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return _m.GetEnumerator();
		}


		public Fragment<TCandle> Pick(DateRangeUtc range)
		{
			// TODO: make it faster
			int startIndex = 0;
			foreach (int i in _m.Length)
			{
				if (_m[i].OpenUtc >= range.StartUTC)
				{
					startIndex = i;
					break;
				}
			}

			int count = 0;
			for (int i = startIndex; i < _m.Length; i++)
			{
				if (_m[i].OpenUtc <= range.EndUTC)
				{
					count = i - startIndex + 1;
				}
				else
				{
					break;
				}
			}

			TCandle[] res = new TCandle[count];
			foreach (int i in count)
			{
				res[i] = _m[i + startIndex];
			}
			return new(res, TimeFrame);
		}


		public static Fragment<TCandle> Empty(TimeFrame tf)
		{
			return new([], tf);
		}


		public Fragment<TCandle> ConvertTimeFrame(TimeFrame targetTimeFrame)
		{
			if (this.TimeFrame.CanBeConvertedTo(targetTimeFrame) == false)
			{
				throw new ArgumentException("The fragment cannot be converted in new timeframe");
			}

			if (TimeFrame == targetTimeFrame)
			{
				return this;
			}

			return new Fragment<TCandle>(
				Fragment<TCandle>.ConvertTimeFrame(_m, targetTimeFrame),
				targetTimeFrame
			);
		}


		public TCandle this[int i]
		{
			get => _m[i];
		}


		public TCandle this[Index i]
		{
			get => _m[i];
		}


		private static TCandle[] ConvertTimeFrame(TCandle[] baseChart, TimeFrame newTimeFrame)
		{
			if (baseChart.Length == 0)
			{
				return baseChart;
			}

			List<TCandle> res = new(baseChart.Length);

			DateTime newOpenTime = newTimeFrame.RoundDateTimeDown(baseChart[0].OpenUtc);
			TCandle buildingCandle = baseChart[0].ShiftOpenUtc(newOpenTime);

			for (int i = 1; i < baseChart.Length; i++)
			{
				TCandle oldCandle = baseChart[i];
				newOpenTime = newTimeFrame.RoundDateTimeDown(oldCandle.OpenUtc);

				if (newOpenTime == buildingCandle.OpenUtc)
				{
					buildingCandle = buildingCandle.MergeWith(oldCandle);
				}
				else if (newOpenTime > buildingCandle.OpenUtc)
				{
					res.Add(buildingCandle);
					buildingCandle = oldCandle.ShiftOpenUtc(newOpenTime);
				}
				else
				{
					// This problem could happen by one of next reasons:
					// 1) The baseChart is not sorted in ascending mode
					// 2) Rounding DateTime works wrong
					// 3) Something wrong with code of the method
					throw new Exception("Something went wrong!");
				}
			}

			res.Add(buildingCandle);
			return [.. res.ToArray()];
		}

	}
}
