namespace LocalCandleBuffer
{
	public interface IStorableCandle<TSelf> where TSelf : IStorableCandle<TSelf>
	{
		DateTime OpenUtc { get; }
		long OpenUnixMs { get; }


		/// <summary>
		/// This method allows Join-method to work correctly
		/// When two fragments has a candle with the same OpenUtc,
		/// we need to decide which candle is more actual
		///
		/// To implement this method you can compare volumes if you store them,
		/// or compare branches (newer candle must be wider) is you don't have volumes.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		bool IsThisCandleNewerThan(TSelf other);


		/// <summary>
		/// This method must be defined for ability to convert time frame.
		/// Open price and time take from THIS candle
		/// Close price take from the OTHER.
		/// Low must be the smallest between two and High must be the greatest
		/// Volumes must be added together
		/// </summary>
		/// <param name="other">A candle next to this</param>
		TSelf MergeWith(TSelf other);


		/// <summary>
		/// Create a new candle with changed OpenUtc date.
		/// Rest of props (prices, volumes) must be the same.
		/// Required for the process of conversion time frame
		/// </summary>
		TSelf ShiftOpenUtc(DateTime newOpenUtc);
	}
}
