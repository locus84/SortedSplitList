using System;
using System.Collections.Generic;
using System.Linq;
using Collection;
using NUnit.Framework;

namespace Test_SortedSplitList
{
    [TestFixture]
    public class UnitTest1
    {
        private static SortedSplitList<TestObject> GetSortedSplitListSortedById()
        {            
            SortedSplitList<TestObject> sortedSplitList = new SortedSplitList<TestObject>(new CompareById());
            var obj1 = new TestObject() { Id = 1 };
            var obj2 = new TestObject() { Id = 2 };
            var obj3 = new TestObject() { Id = 3 };
            var obj4 = new TestObject() { Id = 4 };           
            sortedSplitList.Add(obj2);
            sortedSplitList.Add(obj4);
            sortedSplitList.Add(obj1);
            sortedSplitList.Add(obj3);
            return sortedSplitList;
        }
        public void TestAddItem()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();            
            Assert.AreEqual(4, sortedSplitListSortedById.Count);
            int i = 1;
            foreach (var testObject in sortedSplitListSortedById) {
                Assert.AreEqual(i++, testObject.Id);
            }
        }

        [Test]
        public void TestRemoveItem()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();

            // Act
            sortedSplitListSortedById.Remove(new TestObject(){ Id = 3});
            sortedSplitListSortedById.Remove(new TestObject() { Id = 1 });
            sortedSplitListSortedById.Remove(new TestObject() { Id = 4 });

            // Asert
            Assert.AreEqual(1, sortedSplitListSortedById.Count);
            Assert.AreEqual(2, sortedSplitListSortedById[0].Id);
        }

        [Test]
        public void TestRemoveAll()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            // Act
            sortedSplitListSortedById.RemoveAll(a=>a.Id%2 == 0);
            //Asset
            Assert.AreEqual(2, sortedSplitListSortedById.Count);
            Assert.AreEqual(1, sortedSplitListSortedById[0].Id);
            Assert.AreEqual(3, sortedSplitListSortedById[1].Id);
            
        }

        [Test]
        public void TestClear()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            // Act
            sortedSplitListSortedById.Clear();
            //Asset
            Assert.AreEqual(0, sortedSplitListSortedById.Count);            
        }

        [Test]
        public void TestEmptyEnumeration()
        {
            // Arrange
            var sortedSplitList = new SortedSplitList<TestObject>(new CompareById());
            // Act
            Assert.IsFalse(sortedSplitList.Any());          
        }

        [Test]
        public void TestRetriveExistingItem()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            // Act
            var obj = sortedSplitListSortedById.Retrieve(new TestObject(){ Id = 3});
            //Asset
            Assert.AreEqual(3,obj.Id);   
        }
        [Test]
        public void TestRetrieveNotExistingItem()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            // Act
            var obj = sortedSplitListSortedById.Retrieve(new TestObject() { Id = 10 });
            //Asset
            Assert.IsNull(obj);
        }      
        
        [Test]
        public void TestBinarySearchFound()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            int index = sortedSplitListSortedById.BinarySearch(new TestObject() { Id = 2 });
            Assert.AreEqual(1, index);
        }

        [Test]
        public void TestBinarySearchHightNotFound()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            int index = sortedSplitListSortedById.BinarySearch(new TestObject() { Id = 8 });
            Assert.AreEqual(-5, index);
        }

        [Test]
        public void TestBinarySearchLowNotFound()
        {
            var sortedSplitListSortedById = GetSortedSplitListSortedById();
            int index = sortedSplitListSortedById.BinarySearch(new TestObject() { Id = 0 });
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void TestPartiallyEnumerate()
        {
            var sortedSplitList = new SortedSplitList<TestObject>(new CompareByDateId());            

            sortedSplitList.Add(new TestObject() { Id = 1, Date = DateTime.Parse("01/01/2003") });
            sortedSplitList.Add(new TestObject() { Id = 2, Date = DateTime.Parse("01/01/2003") });
            sortedSplitList.Add(new TestObject() { Id = 3, Date = DateTime.Parse("01/01/2003") });
            sortedSplitList.Add(new TestObject() { Id = 4, Date = DateTime.Parse("01/01/2003") });
            sortedSplitList.Add(new TestObject() { Id = 5, Date = DateTime.Parse("01/02/2003") });
            sortedSplitList.Add(new TestObject() { Id = 6, Date = DateTime.Parse("01/02/2003") });
            sortedSplitList.Add(new TestObject() { Id = 7, Date = DateTime.Parse("01/02/2003") });
            sortedSplitList.Add(new TestObject() { Id = 8, Date = DateTime.Parse("01/03/2003") });
            sortedSplitList.Add(new TestObject() { Id = 9, Date = DateTime.Parse("01/03/2003") });            
            
           Assert.AreEqual(4, sortedSplitList.PartiallyEnumerate(new TestObject() {Date = DateTime.Parse("01/01/2003")}, new CompareByDate()).Count() );
           Assert.AreEqual(3, sortedSplitList.PartiallyEnumerate(new TestObject() { Date = DateTime.Parse("01/02/2003") }, new CompareByDate()).Count());
           Assert.AreEqual(2, sortedSplitList.PartiallyEnumerate(new TestObject() { Date = DateTime.Parse("01/03/2003") }, new CompareByDate()).Count());


           foreach (var testObject in sortedSplitList.PartiallyEnumerate(new TestObject() { Date = DateTime.Parse("01/01/2003") }, new CompareByDate()) )
               Console.WriteLine(testObject.Id);                           

        }             
       
    }
}
