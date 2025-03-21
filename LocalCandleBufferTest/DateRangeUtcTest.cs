using LocalCandleBuffer;

namespace LocalCandleBufferTest
{
	[TestClass]
	public class DateRangeUtcTest
	{
		[TestMethod]
		[DataRow(0, 1, 2, 3, false)]  // No touch
		[DataRow(0, 2, 2, 3, true)]   // Touch
		[DataRow(0, 2, 1, 3, true)]   // Intersection
		[DataRow(0, 3, 1, 3, true)]   // Equal top
		[DataRow(1, 2, 1, 3, true)]   // Equal bot
		[DataRow(0, 4, 1, 3, true)]   // Included
		[DataRow(0, 4, 0, 4, true)]   // Equality
		public void DoesTouch(int start1, int end1, int start2, int end2, bool expected)
		{
			DateRangeUtc range1 = new(IntToDate(start1), IntToDate(end1));
			DateRangeUtc range2 = new(IntToDate(start2), IntToDate(end2));
			Assert.AreEqual(expected, range1.DoesTouch(range2));
			Assert.AreEqual(expected, range2.DoesTouch(range1));

			static DateTime IntToDate(int val)
			{
				return new DateTime(2000, 1, 1, val, 0, 0, DateTimeKind.Utc);
			}
		}
	}
}
