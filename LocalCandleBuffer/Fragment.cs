namespace LocalCandleBuffer
{
	public class Fragment<TCandle> where TCandle : IStorableCandle<TCandle>
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


		public TCandle this[int i]
		{
			get => _m[i];
		}
		public TCandle this[Index i]
		{
			get => _m[i];
		}
	}
}
