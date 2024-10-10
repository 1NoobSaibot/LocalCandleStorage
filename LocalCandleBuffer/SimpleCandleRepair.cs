namespace LocalCandleBuffer
{
	public static class SimpleCandleRepair
	{
		public static IList<ICandleF> Glue(IList<ICandleF> frag1, IList<ICandleF> frag2)
		{
			TryUpdate(frag1, frag2, out frag1);
			return frag1;
		}


		public static IList<ICandleF> RemoveDuplicates(IList<ICandleF> list)
		{
			if (list.Count < 2)
			{
				return list;
			}

			List<ICandleF> repaired = new(list.Count)
			{
				list[0]
			};
			for (int i = 1; i < list.Count; i++)
			{
				var prevCandle = repaired.Last();
				var currentCadnle = list[i];
				if (prevCandle.OpenUnixMc >= currentCadnle.OpenUnixMc)
				{
					if (list[i - 1].OpenUnixMc > list[i].OpenUnixMc)
					{
						throw new Exception($"OMG! It's broken strong enough! {list[i - 1].OpenUtc} is before {list[i].OpenUtc}");
					}

					repaired[^1] = PickNewer(prevCandle, currentCadnle);
				}
				else
				{
					repaired.Add(currentCadnle);
				}
			}

			return repaired;
		}


		public static ICandleF PickNewer(ICandleF mostLikelyOlder, ICandleF mostLikelyNewer)
		{
			if (mostLikelyNewer.IsNewerThan(mostLikelyOlder))
			{
				return mostLikelyNewer;
			}
			return mostLikelyOlder;
		}


		internal static bool TryUpdate(IList<ICandleF> oldFrag, IList<ICandleF> newerFrag, out IList<ICandleF> joined)
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
			joined = new List<ICandleF>(oldFrag.Count + newerFrag.Count);
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

				if (newCandle.IsNewerThan(oldCandle))
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
