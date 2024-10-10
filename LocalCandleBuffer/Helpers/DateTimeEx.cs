namespace LocalCandleBuffer.Helpers
{
	internal static class DateTimeEx
	{
		private const int MicrosecondsPerMillisecond = 1000;
		private const long TicksPerMicrosecond = 10;
		private const long TicksPerMillisecond = TicksPerMicrosecond * MicrosecondsPerMillisecond;
		private const long TicksPerSecond = TicksPerMillisecond * 1000;
		private const long TicksPerMinute = TicksPerSecond * 60;
		private const long TicksPerHour = TicksPerMinute * 60;
		private const long TicksPerDay = TicksPerHour * 24;


		public static DateTime FromUnixTimeMilliseconds(long unixTimeMilliseconds)
		{
			return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).UtcDateTime;
		}


		public static bool IsRoundedToMinutes(this DateTime value)
		{
			return value.Ticks % TicksPerMinute == 0;
		}


		public static DateTime RoundDownToMinutes(this DateTime input)
		{
			return new DateTime(
				input.Ticks - input.Ticks % TicksPerMinute,
				input.Kind
			);
		}


		public static DateTime RoundDownToYear(this DateTime input)
		{
			return new DateTime(
				year: input.Year,
				month: 1,
				day: 1,
				hour: 0,
				minute: 0,
				second: 0,
				kind: input.Kind
			);
		}
	}
}
