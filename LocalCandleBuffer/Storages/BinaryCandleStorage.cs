
namespace LocalCandleBuffer.Storages
{
	public abstract class BinaryCandleStorage<TCandle> : ICandleStorage<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _path;


		public BinaryCandleStorage(string path)
		{
			_path = path;
		}


		public Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req)
		{
			if (File.Exists(_path) == false)
			{
				return Task.FromResult<Fragment<TCandle>>(new([], TimeFrame.OneMinute));
			}

			using BinaryReader reader = new(File.Open(_path, FileMode.Open));
			List<TCandle> candles = new((int)(reader.BaseStream.Length / BytesPerCandle()));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var candle = ReadSingleCandleFromFile(reader);

				if (candle.OpenUtc < req.StartUTC)
				{
					continue;
				}
				if (candle.OpenUtc >= req.EndUTC)
				{
					break;
				}

				candles.Add(candle);
			}

			return Task.FromResult<Fragment<TCandle>>(new([.. candles], TimeFrame.OneMinute));
		}


		protected abstract TCandle ReadSingleCandleFromFile(BinaryReader reader);
		protected abstract void WriteSingleCandleToFile(BinaryWriter writer, TCandle candle);
		protected abstract int BytesPerCandle();


		public async Task UpdateAndSave(Fragment<TCandle> newCandles)
		{
			if (newCandles.IsEmpty)
			{
				return;
			}

			Fragment<TCandle> oldCandles = await ReadAll();
			Fragment<TCandle> allCandles = oldCandles.Join(newCandles);

			if (allCandles.Count > oldCandles.Count)
			{
				using BinaryWriter writer = new(File.Open(_path, FileMode.Create));
				foreach (TCandle candle in newCandles)
				{
					WriteSingleCandleToFile(writer, candle);
				}

				writer.Flush();
			}

			return;
		}


		private Task<Fragment<TCandle>> ReadAll()
		{
			return Get1mCandles(DateRangeUtc.All(TimeFrame.OneMinute));
		}
	}
}
