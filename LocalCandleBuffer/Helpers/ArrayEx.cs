namespace LocalCandleBuffer.Helpers
{
	internal static class ArrayEx
	{
		/// <summary>
		/// This method joins two arrays. If some elements are included once in array a and once in array b,
		/// they will not be duplicated in returned array
		/// 
		/// WARNING: The method DOES NOT clear duplicates inside the input arrays!
		/// If any or both input arrays have duplicates, you'll most likely get duplicates in the result array.
		/// 
		/// WARNING: Also returned array is not sorted.
		/// </summary>
		public static T[] JoinUnique<T>(T[] a, T[] b)
		{
			if (a.Length == 0)
			{
				return b;
			}
			if (b.Length == 0)
			{
				return a;
			}

			List<T> res = new(a.Length + b.Length);
			res.AddRange(a);

			foreach (T t in b)
			{
				if (res.Contains(t))
				{
					continue;
				}
				res.Add(t);
			}

			return [.. res];
		}
	}
}
