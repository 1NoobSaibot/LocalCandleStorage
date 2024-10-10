namespace LocalCandleBuffer
{
	internal class LimitedCandleStorage<TCandle>
		: BinaryCandleStorage<TCandle>
		where TCandle : IStorableCandle<TCandle>
	{
		public readonly DateTime MinAvailableOpenUtc;
		public readonly DateTime StrictMaxOpenUtc;


		public LimitedCandleStorage(
			ICandleWritterReader<TCandle> readerWriter,
			string path,
			DateTime minAvailableOpenUtc,
			DateTime strictMaxOpenUtc
		)
			: base(readerWriter, path)
		{
			if (
				minAvailableOpenUtc.Kind != DateTimeKind.Utc
				|| strictMaxOpenUtc.Kind != DateTimeKind.Utc
			)
			{
				throw new ArgumentException("All time params must be UTC");
			}
			if (minAvailableOpenUtc >= strictMaxOpenUtc)
			{
				throw new ArgumentException("Invalid range");
			}

			MinAvailableOpenUtc = minAvailableOpenUtc;
			StrictMaxOpenUtc = strictMaxOpenUtc;
		}


		public override void Save(IList<TCandle> candles)
		{
			List<TCandle> selected = new(candles.Count);
			foreach (var candle in candles)
			{
				if (
					candle.OpenUtc >= MinAvailableOpenUtc
					&& candle.OpenUtc < StrictMaxOpenUtc
				)
				{
					selected.Add(candle);
				}
			}

			base.Save(selected);
		}
	}
}
