namespace LocalCandleBuffer
{
	public readonly struct TimeFrame
	{
		public static readonly TimeFrame OneMinute = new(TimeSpan.FromMinutes(1));

		public readonly TimeSpan AsTimeSpan;


		public TimeFrame(TimeSpan timeSpan)
		{
			if (timeSpan <= TimeSpan.Zero)
			{
				throw new ArgumentException("Negative or zero TimeSpan cannot be a valid TimeFrame");
			}

			AsTimeSpan = timeSpan;
		}


		public bool IsValidDate(DateTime endDateUtc)
		{
			return (endDateUtc.Ticks % AsTimeSpan.Ticks) == 0;
		}


		public static bool operator ==(TimeFrame l, TimeFrame r)
		{
			return l.AsTimeSpan == r.AsTimeSpan;
		}


		public static bool operator !=(TimeFrame l, TimeFrame r)
		{
			return l.AsTimeSpan != r.AsTimeSpan;
		}


		public override bool Equals(object? obj)
		{
			if (obj is TimeFrame tf)
			{
				return this == tf;
			}
			return false;
		}


		public override int GetHashCode()
		{
			return AsTimeSpan.GetHashCode();
		}
	}
}
