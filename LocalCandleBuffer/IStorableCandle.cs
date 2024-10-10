namespace LocalCandleBuffer
{
	public interface IStorableCandle<TSelf> where TSelf : IStorableCandle<TSelf>
	{
		DateTime OpenUtc { get; }
		long OpenUnixMc { get; }

		bool IsThisVolumeBiggerThan(TSelf other);
	}
}
