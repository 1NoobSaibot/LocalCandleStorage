using LocalCandleBuffer.Types;
using SafeFile;
using SafeFile.Atomic;

namespace LocalCandleBuffer.Storages
{
	public abstract class BinaryCandleStorage<TCandle> : ICandleStorage<TCandle>
		where TCandle : IStorableCandle<TCandle>
	{
		private readonly AtomicFileStorage _atomicFile;
		public readonly TimeFrame BaseTimeFrame;
		public string Path => _atomicFile.FileName;


		public BinaryCandleStorage(string path, TimeFrame baseTimeFrame)
		{
			_atomicFile = new(path);
			BaseTimeFrame = baseTimeFrame;
		}


		public Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req)
		{
			if (_atomicFile.TryRead<Fragment<TCandle>>(
				ReadCandlesFn,
				out Fragment<TCandle>? readFragment
			))
			{
				return Task.FromResult(readFragment!);
			}

			return Task.FromResult(Fragment<TCandle>.Empty(BaseTimeFrame));

			Fragment<TCandle> ReadCandlesFn(SignedBinaryReader reader)
			{
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

				return new Fragment<TCandle>([.. candles], BaseTimeFrame);
			}
		}


		public Task<Fragment<TCandle>> Get1mCandles(DateRangeUtc req, Limit limit)
		{
			throw new NotImplementedException();
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
				_atomicFile.WriteAndSave((writer) =>
				{
					foreach (TCandle candle in newCandles)
					{
						WriteSingleCandleToFile(writer, candle);
					}
				});
			}

			return;
		}


		private Task<Fragment<TCandle>> ReadAll()
		{
			return Get1mCandles(DateRangeUtc.All(BaseTimeFrame));
		}
	}
}
