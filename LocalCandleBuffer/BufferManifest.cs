namespace LocalCandleBuffer
{
	internal class BufferManifest
	{
		private readonly string _filePath;
		private Dictionary<string, CandleRange>? _ranges;


		public BufferManifest(string filePath)
		{
			_filePath = filePath;
		}


		public CandleRange? GetBufferedRangeData(string symbolId)
		{
			LoadIfNeed();
			_ranges!.TryGetValue(symbolId, out var rangeData);
			return rangeData;
		}


		public void UpdateRangeData(string symbol, IList<ICandleF> candles)
		{
			if (candles.Count == 0)
			{
				return;
			}
			UpdateRangeData(symbol, new CandleRange(candles[0].OpenUtc, candles.Last().OpenUtc));
		}


		public void UpdateRangeData(string symbolId, CandleRange newRange)
		{
			LoadIfNeed();

			if (_ranges!.ContainsKey(symbolId))
			{
				CandleRange oldRange = _ranges[symbolId];
				/*if (
					oldRange.DoesTouch(newRange) == false
				)
				{
					throw new Exception("Ranges must touch each other");
				}*/

				DateTime newStart, newEnd;
				if (newRange.StartUTC < oldRange.StartUTC)
				{
					newStart = newRange.StartUTC;
				}
				else
				{
					newStart = oldRange.StartUTC;
				}

				if (newRange.EndUTC > oldRange.EndUTC)
				{
					// To prevent saving and never updating the last candle
					// We don't save the last minute
					newEnd = newRange.EndUTC.AddMinutes(-1);

					// And if we move the last limit, we can have no range to save.
					// It's improbable, but you have to check it
					if (newEnd < newStart)
					{
						return;
					}
				}
				else
				{
					// No need to move oldEnd, because it was already moved once
					newEnd = oldRange.EndUTC;
				}
				_ranges[symbolId] = new(newStart, newEnd);
			}
			else
			{
				if (newRange.StartUTC == newRange.EndUTC)
				{
					return;
				}

				_ranges.Add(
					symbolId,
					new(newRange.StartUTC, newRange.EndUTC.AddMinutes(-1))
				);
			}

			Save();
		}


		private void LoadIfNeed()
		{
			_ranges ??= Load(_filePath);
		}


		private static Dictionary<string, CandleRange> Load(string fileName)
		{
			Dictionary<string, CandleRange> res = [];
			if (File.Exists(fileName) == false)
			{
				return res;
			}

			using BinaryReader reader = new(new FileStream(fileName, FileMode.Open));
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				string symbolId = reader.ReadString();
				DateTime startUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
				DateTime endUtc = new(reader.ReadInt64(), DateTimeKind.Utc);
				res.Add(symbolId, new(startUtc, endUtc));
			}
			return res;
		}


		private void Save()
		{
			if (_ranges is null || _ranges.Count == 0)
			{
				return;
			}

			using BinaryWriter writer = new(new FileStream(_filePath, FileMode.Create));
			foreach (var key in _ranges.Keys)
			{
				var range = _ranges[key];
				writer.Write(key);
				writer.Write(range.StartUTC.Ticks);
				writer.Write(range.EndUTC.Ticks);
			}
			writer.Flush();
		}
	}
}
