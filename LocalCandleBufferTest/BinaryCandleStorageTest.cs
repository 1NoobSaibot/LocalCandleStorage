using LocalCandleBuffer;
using LocalCandleBuffer.Storages;
using LocalCandleBufferTest.Fakes;
using LocalCandleBufferTest.Implementations;

namespace LocalCandleBufferTest
{
	[TestClass]
	public class BinaryCandleStorageTest
	{
		[TestMethod]
		public async Task WritesAndReads()
		{
			File.Delete("testCandles.bin");

			Fragment<Candle> candles = await FakeExchangeApi.Instance.GetAllCandles();

			Assert.IsNotNull(candles);
			Assert.IsTrue(candles.Count >= 2);

			BinaryCandleStorage<Candle> storage = new FileStorage("testCandles.bin", TimeFrame.OneMinute);
			await storage.UpdateAndSave(candles);

			var readCandles = await storage.GetCandles(DateRangeUtc.All(TimeFrame.OneMinute));
			Assert.AreEqual(candles.Count, readCandles.Count);
			foreach (int i in readCandles.Count)
			{
				Assert.AreEqual(candles[i].Open, readCandles[i].Open);
				Assert.AreEqual(candles[i].High, readCandles[i].High);
				Assert.AreEqual(candles[i].Low, readCandles[i].Low);
				Assert.AreEqual(candles[i].Close, readCandles[i].Close);
				Assert.AreEqual(candles[i].OpenUtc, readCandles[i].OpenUtc);
			}
		}
	}
}