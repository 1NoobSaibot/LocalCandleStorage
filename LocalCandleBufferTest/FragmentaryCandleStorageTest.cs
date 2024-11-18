using LocalCandleBuffer.Storages.Fragmented;
using LocalCandleBufferTest.Fakes;
using LocalCandleBufferTest.Implementations;

namespace LocalCandleBufferTest
{
	[TestClass]
	public class FragmentaryCandleStorageTest
	{
		[TestMethod]
		public async Task WritesAndReads()
		{
			var candles = await FakeExchangeApi.Instance.GetAllCandles();
			Assert.IsNotNull(candles);
			Assert.IsTrue(candles.Count >= 4200);
#pragma warning disable IDE0059
			var firstApiCandle = candles.First();
			var lastApiCandle = candles.Last();

			FragmentedCandleStorage<Candle> storage = new FragmentedStorage("testFragmentaryCandleFolderV2");
			await storage.Save(candles);
			var readCandles = await storage.Get1mCandles(FakeExchangeApi.AvailableRange);
			var firstStorageCandle = readCandles.First();
			var lastStorageCandle = readCandles.Last();
#pragma warning restore IDE0059

			for (int i = 1; i < readCandles.Count; i++)
			{
				if (readCandles[i - 1].OpenUnixMc >= readCandles[i].OpenUnixMc)
				{
					Assert.Fail();
				}
			}

			int smallestCount = Math.Min(candles.Count, readCandles.Count);
			for (int i = 0; i < smallestCount; i++)
			{
				var candle = candles[i];
				var readCandle = readCandles[i];
				Assert.AreEqual(candle.OpenUtc, readCandle.OpenUtc);
				Assert.AreEqual(candle.Open, readCandle.Open);
				Assert.AreEqual(candle.High, readCandle.High);
				Assert.AreEqual(candle.Low, readCandle.Low);
				Assert.AreEqual(candle.Close, readCandle.Close);
			}

			Assert.AreEqual(candles.Count, readCandles.Count);
		}
	}
}