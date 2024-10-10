using LocalCandleBuffer;
using LocalCandleBufferTest.Fakes;

namespace LocalCandleBufferTest
{
	[TestClass]
	public class BinaryCandleStorageTest
	{
		[TestMethod]
		public void WritesAndReads()
		{
			File.Delete("testCandles.bin");

			var candles = FakeExchangeApi.Instance.GetAllCandles();
			Assert.IsNotNull(candles);
			Assert.IsTrue(candles.Count >= 2);

			BinaryCandleStorage storage = new("testCandles.bin");
			storage.Save(candles);

			var readCandles = storage.ReadAll();
			Assert.AreEqual(candles.Count, readCandles.Count);
			for (int i = 0; i < readCandles.Count; i++)
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