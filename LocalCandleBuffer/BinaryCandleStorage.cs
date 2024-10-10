using LocalCandleBuffer.Helpers;

namespace LocalCandleBuffer
{
	public class BinaryCandleStorage
	{
		private readonly string _path;


		public BinaryCandleStorage(string path)
		{
			_path = path;
		}


		public IList<ICandleF> Read(CandleRange req)
		{
			if (File.Exists(_path) == false)
			{
				return Array.Empty<ICandleF>();
			}

			using BinaryReader reader = new(File.Open(_path, FileMode.Open));
			List<ICandleF> candles = new((int)(reader.BaseStream.Length / 24));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var candle = new StoredCandle(reader);

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


		public IList<ICandleF> ReadAll()
		{
			if (File.Exists(_path) == false)
			{
				return Array.Empty<ICandleF>();
			}

			using BinaryReader reader = new(File.Open(_path, FileMode.Open));
			List<ICandleF> candles = new((int)(reader.BaseStream.Length / 24));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var candle = new StoredCandle(reader);
				candles.Add(candle);
			}

			return candles;
		}


		public virtual void Save(IList<ICandleF> candles)
		{
			var oldCandles = ReadAll();
			if (oldCandles.Count > 0)
			{
				candles = SimpleCandleRepair.Glue(oldCandles, candles);
			}

			using BinaryWriter writer = new(File.Open(_path, FileMode.Create));
			foreach (var candle in candles)
			{
				StoredCandle.WriteToFile(candle, writer);
			}

			writer.Flush();
		}


		private class StoredCandle : ICandleF
		{
			public float Open { get; }
			public float High { get; }
			public float Low { get; }
			public float Close { get; }
			public long OpenUnixMc { get; }
			public DateTime OpenUtc => DateTimeEx.FromUnixTimeMilliseconds(OpenUnixMc);
			public float VolumeBase { get; }
			public float VolumeQuote { get; }


			public StoredCandle(BinaryReader r)
			{
				Open = r.ReadSingle();
				High = r.ReadSingle();
				Low = r.ReadSingle();
				Close = r.ReadSingle();
				OpenUnixMc = r.ReadInt64();
				VolumeBase = r.ReadSingle();
				VolumeQuote = r.ReadSingle();
			}


			public static void WriteToFile(ICandleF candle, BinaryWriter w)
			{
				w.Write(candle.Open);
				w.Write(candle.High);
				w.Write(candle.Low);
				w.Write(candle.Close);
				w.Write(candle.OpenUnixMc);
				w.Write(candle.VolumeBase);
				w.Write(candle.VolumeQuote);
			}
		}
	}
}
