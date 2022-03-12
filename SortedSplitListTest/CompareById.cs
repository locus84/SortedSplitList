using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_SortedSplitList
{
    public class CompareById : IComparer<TestObject>
    {
        public int Compare(TestObject x, TestObject y)
        {
            if (x.Id == y.Id)
                return 0;
            if (x.Id < y.Id)
                return -1;
            return 1;
        }
    }
}
