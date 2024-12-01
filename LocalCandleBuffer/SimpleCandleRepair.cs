namespace LocalCandleBuffer
{
	public static class SimpleCandleRepair
	{
		public static IList<TCandle> Glue<TCandle>(
			IList<TCandle> frag1,
			IList<TCandle> frag2
		) where TCandle : IStorableCandle<TCandle>
		{
			TryUpdate(frag1, frag2, out frag1);
			return frag1;
		}


		internal static bool TryUpdate<TCandle>(
			IList<TCandle> oldFrag,
			IList<TCandle> newerFrag,
			out IList<TCandle> joined
		) where TCandle : IStorableCandle<TCandle>
		{
			if (oldFrag.Count == 0)
			{
				joined = newerFrag;
				return newerFrag.Count > 0;
			}
			if (newerFrag.Count == 0)
			{
				joined = oldFrag;
				return false;
			}

			int indexOld = 0;
			int indexNew = 0;
			joined = new List<TCandle>(oldFrag.Count + newerFrag.Count);
			bool wereUpdated = false;
			do
			{
				var oldCandle = oldFrag[indexOld];
				var newCandle = newerFrag[indexNew];

				if (oldCandle.OpenUnixMc < newCandle.OpenUnixMc)
				{
					joined.Add(oldCandle);
					indexOld++;
					continue;
				}
				if (newCandle.OpenUnixMc < oldCandle.OpenUnixMc)
				{
					joined.Add(newCandle);
					indexNew++;
					wereUpdated = true;
					continue;
				}

				if (newCandle.IsThisCandleNewerThan(oldCandle))
				{
					joined.Add(newCandle);
					wereUpdated = true;
				}
				else
				{
					joined.Add(oldCandle);
				}
				indexOld++;
				indexNew++;
			} while (
				indexOld < oldFrag.Count
				&& indexNew < newerFrag.Count
			);

			if (indexNew < newerFrag.Count)
			{
				wereUpdated = true;
				do
				{
					joined.Add(newerFrag[indexNew]);
					indexNew++;
				} while (indexNew < newerFrag.Count);
			}
			else if (indexOld < oldFrag.Count)
			{
				do
				{
					joined.Add(oldFrag[indexOld]);
					indexOld++;
				} while (indexOld < oldFrag.Count);
			}

			return wereUpdated;
		}
	}
}
