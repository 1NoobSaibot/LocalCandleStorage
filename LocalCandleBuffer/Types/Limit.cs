﻿
namespace LocalCandleBuffer.Types
{
	public readonly struct Limit
	{
		public readonly bool LoadFromEnd;
		public bool LoadFromStart => LoadFromEnd == false;
		public readonly int OrientedCount;


		private Limit(int count, bool fromTheEnd)
		{
			if (count <= 0)
			{
				throw new ArgumentException("Count must be non zero positive number");
			}
			LoadFromEnd = fromTheEnd;
			OrientedCount = count;
		}


		public static Limit FromTheEnd(int count)
		{
			return new(count, fromTheEnd: true);
		}


		public static Limit FromTheStart(int count)
		{
			return new(count, fromTheEnd: false);
		}


		public static Limit MaxFromTheEnd()
		{
			return new(int.MaxValue, fromTheEnd: true);
		}


		public static Limit MaxFromTheStart()
		{
			return new(int.MaxValue, fromTheEnd: false);
		}


		public Limit Cut(int count)
		{
			return new Limit(OrientedCount - count, fromTheEnd: LoadFromEnd);
		}
	}
}
