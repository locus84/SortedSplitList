using System.Collections.Generic;

namespace Test_SortedSplitList
{
    public class CompareByDate : IComparer<TestObject>
    {
        public int Compare(TestObject x, TestObject y)
        {
            return x.Date.CompareTo(y.Date);
        }
    }
}