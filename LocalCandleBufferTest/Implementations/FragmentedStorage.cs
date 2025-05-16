using LocalCandleBuffer;
using LocalCandleBuffer.Storages;
using LocalCandleBuffer.Storages.Fragmented;

namespace LocalCandleBufferTest.Implementations
{
	internal class FragmentedStorage : FragmentedCandleStorage<Candle>
	{
		public FragmentedStorage(string pathToFolder, TimeFrame baseTimeFrame)
			: base(pathToFolder, baseTimeFrame)
		{
		}

		protected override ICandleStorage<Candle> CreateStorage(string path)
		{
			return new FileStorage(path, BaseTimeFrame);
		}
	}
}
