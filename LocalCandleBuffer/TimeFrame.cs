namespace LocalCandleBuffer
{
	public readonly struct TimeFrame
	{
		public static readonly TimeFrame OneMinute = new(TimeSpan.FromMinutes(1));
		public static readonly TimeFrame ThreeMinutes = new(TimeSpan.FromMinutes(3));
		public static readonly TimeFrame FiveMinutes = new(TimeSpan.FromMinutes(5));
		public static readonly TimeFrame FifteenMinutes = new(TimeSpan.FromMinutes(15));
		public static readonly TimeFrame ThirtyMinutes = new(TimeSpan.FromMinutes(30));
		public static readonly TimeFrame OneHour = new(TimeSpan.FromHours(1));
		public static readonly TimeFrame TwoHours = new(TimeSpan.FromHours(2));
		public static readonly TimeFrame FourHours = new(TimeSpan.FromHours(4));
		public static readonly TimeFrame SixHours = new(TimeSpan.FromHours(6));
		public static readonly TimeFrame TwelveHours = new(TimeSpan.FromHours(12));
		public static readonly TimeFrame OneDay = new(TimeSpan.FromDays(1));
		public static readonly TimeFrame ThreeDays = new(TimeSpan.FromDays(3));
		public static readonly TimeFrame OneWeek = new(TimeSpan.FromDays(7));
		public readonly TimeSpan AsTimeSpan;
		public long Ticks => AsTimeSpan.Ticks;

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


		public bool CanBeConvertedTo(TimeFrame targetTimeFrame)
		{
			return (targetTimeFrame.Ticks % this.Ticks) == 0;
		}


		public DateTime RoundDateTimeDown(DateTime dateTime)
		{
			long dtTicks = dateTime.Ticks;
			return new DateTime(
				ticks: dtTicks - (dtTicks % Ticks),
				kind: dateTime.Kind
			);
		}
	}
}
