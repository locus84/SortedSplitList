using System.Collections.Generic;

namespace Test_SortedSplitList
{
    public class CompareByDateId : IComparer<TestObject>
    {
        public int Compare(TestObject x, TestObject y)
        {
            int dateResult = x.Date.CompareTo(y.Date);
            if (dateResult == 0) {
                if (x.Id == y.Id)
                    return 0;
                if (x.Id < y.Id)
                    return -1;
                return 1;
            }
            return dateResult;
        }
    }
}