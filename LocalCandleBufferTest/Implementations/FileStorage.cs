using LocalCandleBuffer.Storages;

namespace LocalCandleBufferTest.Implementations
{
	internal class FileStorage : BinaryCandleStorage<Candle>
	{
		public FileStorage(string path)
			: base(path)
		{ }


		protected override int BytesPerCandle()
		{
			return 32;
		}


		protected override Candle ReadSingleCandleFromFile(BinaryReader reader)
		{
			return new Candle(
				open: reader.ReadSingle(),
				high: reader.ReadSingle(),
				low: reader.ReadSingle(),
				close: reader.ReadSingle(),
				openUnixMc: reader.ReadInt64(),
				volumeBase: reader.ReadSingle(),
				volumeQuote: reader.ReadSingle()
			);
		}


		protected override void WriteSingleCandleToFile(BinaryWriter writer, Candle candle)
		{
			writer.Write(candle.Open);
			writer.Write(candle.High);
			writer.Write(candle.Low);
			writer.Write(candle.Close);
			writer.Write(candle.OpenUnixMc);
			writer.Write(candle.VolumeBase);
			writer.Write(candle.VolumeQuote);
		}
	}
}
