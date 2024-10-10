namespace LocalCandleBuffer
{
	internal class BinaryCandleStorage<TCandle> where TCandle : IStorableCandle<TCandle>
	{
		private readonly string _path;
		private readonly ICandleWritterReader<TCandle> _readerWriter;


		internal BinaryCandleStorage(ICandleWritterReader<TCandle> candleReaderWriter, string path)
		{
			_path = path;
			_readerWriter = candleReaderWriter;
		}


		public IList<TCandle> Read(CandleRange req)
		{
			if (File.Exists(_path) == false)
			{
				return Array.Empty<TCandle>();
			}

			using BinaryReader reader = new(File.Open(_path, FileMode.Open));
			List<TCandle> candles = new((int)(reader.BaseStream.Length / 24));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var candle = _readerWriter.ReadSingleCandleFromFile(reader);

				if (candle.OpenUtc < req.StartUTC)
				{
					continue;
				}
				if (candle.OpenUtc > req.EndUTC)
				{
					break;
				}

				candles.Add(candle);
			}

			return candles;
		}


		public IList<TCandle> ReadAll()
		{
			if (File.Exists(_path) == false)
			{
				return Array.Empty<TCandle>();
			}

			using BinaryReader reader = new(File.Open(_path, FileMode.Open));
			List<TCandle> candles = new((int)(reader.BaseStream.Length / 24));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var candle = _readerWriter.ReadSingleCandleFromFile(reader);
				candles.Add(candle);
			}

			return candles;
		}


		public virtual void Save(IList<TCandle> candles)
		{
			var oldCandles = ReadAll();
			if (oldCandles.Count > 0)
			{
				candles = SimpleCandleRepair.Glue(oldCandles, candles);
			}

			using BinaryWriter writer = new(File.Open(_path, FileMode.Create));
			foreach (var candle in candles)
			{
				_readerWriter.WriteSingleCandleToFiler(candle, writer);
			}

			writer.Flush();
		}
	}
}
