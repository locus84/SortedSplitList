/* [06/06/2013]
 * 
 * SortedSplitList, by Aurélien BOUDOUX.
 * Blog : http://aurelien.boudoux.fr
 * Twitter : @AurelienBoudoux
 * 
 */
using System.Collections;
using System.Data;

namespace Collection
{
    public class SortedSplitList<T> : IEnumerable<T>
    {
        internal sealed class VerticalIndexNode<TNode>
        {
            public TNode FirstItem;
            public List<TNode> List = new List<TNode>();
            public int BeginIndex;
        }
        internal sealed class CompareByFirstItem : IComparer<VerticalIndexNode<T>>
        {
            public IComparer<T> LocalComparer = null;

            public CompareByFirstItem(IComparer<T> defaultComparer)
            {
                LocalComparer = defaultComparer;
            }

            public int Compare(VerticalIndexNode<T> x, VerticalIndexNode<T> y)
            {
                return LocalComparer.Compare(x.FirstItem, y.FirstItem);
            }
        }
        internal sealed class CompareByBeginIndex : IComparer<VerticalIndexNode<T>>
        {
            public int Compare(VerticalIndexNode<T> x, VerticalIndexNode<T> y)
            {
                return x.BeginIndex - y.BeginIndex;
            }
        }
        private readonly IComparer<T> _defaultComparer;
        private readonly List<VerticalIndexNode<T>> _verticalIndex = new List<VerticalIndexNode<T>>();
        private readonly CompareByFirstItem _verticalComparer;
        private readonly CompareByBeginIndex _indexComparer = new CompareByBeginIndex();
        private int _count;
        private bool _isDirty;
        private readonly int _deepness;
        public T this[int i]
        {
            get
            {
                if (i < 0) throw new IndexOutOfRangeException("L'index est négatif.");
                RecalcIndexerIfDirty();
                int index = GetHorizontalTable(default, i, null);
                int beginIndex = _verticalIndex[index].BeginIndex;
                List<T> currentList = _verticalIndex[index].List;
                return currentList[i - beginIndex];
            }
        }
        public int Count { get { return _count; } }
        public SortedSplitList(IComparer<T> defaultComparer, int deepness = 1000)
        {
            if (defaultComparer == null)
                throw new ArgumentNullException();

            _defaultComparer = defaultComparer;
            _verticalComparer = new CompareByFirstItem(_defaultComparer);
            _deepness = deepness;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _verticalIndex.SelectMany(verticalIndexNode => verticalIndexNode.List).GetEnumerator();
        }
        public void Add(T item)
        {
            if (_verticalIndex.Count == 0)
            {
                _verticalIndex.Add(new VerticalIndexNode<T>());
                _verticalIndex[0].List.Add(item);
                _verticalIndex[0].FirstItem = item;
            }
            else
            {
                InternalAdd(item);
            }
            _count++;
            _isDirty = true;
        }
        public void Remove(T item)
        {
            bool deletedNode = false;
            int vi = GetHorizontalTable(item, -1, null);
            List<T> currentTable = _verticalIndex[vi].List;

            int positionToRemove = FastBinarySearch(currentTable, item, _defaultComparer);
            if (positionToRemove >= 0)
            {

                currentTable.RemoveAt(positionToRemove);

                if (currentTable.Count == 0)
                {
                    _verticalIndex.RemoveAt(vi);
                    deletedNode = true;
                }
                else
                if (vi > 0 && (_verticalIndex[vi - 1].List.Count + currentTable.Count) < _deepness)
                {
                    _verticalIndex[vi - 1].List.AddRange(currentTable);
                    currentTable.Clear();
                    _verticalIndex.RemoveAt(vi);
                    deletedNode = true;
                }

                if (deletedNode == false && positionToRemove == 0)
                    _verticalIndex[vi].FirstItem = _verticalIndex[vi].List[0];

                _count--;
                _isDirty = true;
            }
            else
                throw new DeletedRowInaccessibleException("L'element que vous essayez de supprimer est introuvable");

        }
        public void RemoveAll(Predicate<T> match)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var obj = this[i];
                if (match(obj))
                {
                    Remove(obj);
                    _isDirty = false;
                }
            }
            _isDirty = true;
        }
        public void Clear()
        {
            foreach (VerticalIndexNode<T> node in _verticalIndex)
                node.List.Clear();

            _verticalIndex.Clear();
            _verticalIndex.TrimExcess();
            _count = 0;
            _isDirty = true;
        }
        public int BinarySearch(T comparisonItem, IComparer<T> comparer = null)
        {
            RecalcIndexerIfDirty();
            int vIndex = GetHorizontalTable(comparisonItem, -1, comparer ?? _defaultComparer);
            if (vIndex < 0)
                return vIndex;
            int begin = _verticalIndex[vIndex].BeginIndex;
            List<T> internalArray = _verticalIndex[vIndex].List;
            int realIndex = FastBinarySearch(internalArray, comparisonItem, comparer ?? _defaultComparer);
            return (realIndex >= 0) ? realIndex + begin : realIndex - begin;
        }
        public IEnumerable<T> PartiallyEnumerate(T comparisonItem, IComparer<T> comparer = null)
        {
            IComparer<T> currentComparer = comparer ?? _defaultComparer;
            int index = BinarySearch(comparisonItem, comparer);
            if (index >= 0)
            {
                for (; index > 0 && currentComparer.Compare(comparisonItem, this[index - 1]) == 0; index--) ;
                for (; index < Count && currentComparer.Compare(comparisonItem, this[index]) == 0; index++)
                    yield return this[index];
            }
        }
        public T Retrieve(T comparisonItem, IComparer<T> comparer = null)
        {
            int index = BinarySearch(comparisonItem, comparer ?? _defaultComparer);
            if (index >= 0) return this[index];
            return default(T);
        }
        private int FastBinarySearch(List<T> array, T value, IComparer<T> comparer)
        {
            int num = 0;
            int num2 = array.Count - 1;
            while (num <= num2)
            {
                int num3 = num + ((num2 - num) >> 1);
                int num4 = comparer.Compare(array[num3], value);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~num;
        }
        private int GetHorizontalTable(T containItem, int containIndex, IComparer<T> comparer)
        {
            var localStruct = new VerticalIndexNode<T> { FirstItem = containItem, BeginIndex = containIndex };

            if (comparer != null)
                _verticalComparer.LocalComparer = comparer;

            int index = _verticalIndex.BinarySearch(localStruct, (containIndex < 0) ? _verticalComparer : (IComparer<VerticalIndexNode<T>>)_indexComparer);

            if (comparer != null)
                _verticalComparer.LocalComparer = _defaultComparer;

            if (index < 0)
            {
                if (~index < _verticalIndex.Count)
                {

                    if (~index <= 1)
                        return 0;
                    return ~index - 1;
                }
                return _verticalIndex.Count - 1;
            }
            return index;
        }
        private void InternalAdd(T item)
        {
            int vi = GetHorizontalTable(item, -1, null);
            List<T> currentTable = _verticalIndex[vi].List;

            int position = FastBinarySearch(currentTable, item, _defaultComparer);

            if (position < 0)
            {
                if (~position < currentTable.Count)
                {
                    position = ~position;
                }
                else
                    position = -1;
            }

            if (currentTable.Count < _deepness)
            {
                if (position == 0)
                {
                    _verticalIndex[vi].FirstItem = item;
                    currentTable.Insert(position, item);
                }
                else if (position > 0)
                    currentTable.Insert(position, item);
                else
                    currentTable.Add(item);
                return;
            }

            int median = _deepness >> 1;
            if (position >= 0 && position <= median)
            { // si l'insertion se fait dans la partie A

                // si nous somme sur la derniere table, ou que la table suivante ne peut pas accueillir
                // la moitier B, nous créont un nouveau noeud
                if (vi == _verticalIndex.Count - 1 || _verticalIndex[vi + 1].List.Count > median)
                {
                    _verticalIndex.Insert(vi + 1, new VerticalIndexNode<T>());
                }

                // deplacement de B dans le noeud d'en dessous
                _verticalIndex[vi + 1].List.InsertRange(0, currentTable.GetRange(median, currentTable.Count - median));
                currentTable.RemoveRange(median, currentTable.Count - median);

                // insertion de x dans A
                currentTable.Insert(position, item);

                // mise à jour des indexs
                _verticalIndex[vi + 1].FirstItem = _verticalIndex[vi + 1].List[0];
                if (position == 0)
                    _verticalIndex[vi].FirstItem = _verticalIndex[vi].List[0];

            }
            else
            { // sinon, l'insertion se fait dans la partie B

                // si nous somme sur la derniere table, ou que la table suivante ne peut pas accueillir
                // la moitier B, nous créont un nouveau noeud
                if (vi == _verticalIndex.Count - 1 || _verticalIndex[vi + 1].List.Count > median)
                {
                    _verticalIndex.Insert(vi + 1, new VerticalIndexNode<T>());
                }

                // insertion de x dans B
                if (position > 0)
                    currentTable.Insert(position, item);
                else
                    currentTable.Add(item);

                // deplacement de B dans le noeud d'en dessous
                _verticalIndex[vi + 1].List.InsertRange(0, currentTable.GetRange(median, currentTable.Count - median));
                currentTable.RemoveRange(median, currentTable.Count - median);

                // mise à jour des indexs
                _verticalIndex[vi + 1].FirstItem = _verticalIndex[vi + 1].List[0];
                if (position == 0)
                    _verticalIndex[vi].FirstItem = _verticalIndex[vi].List[0];
            }
        }
        private void RecalcIndexerIfDirty()
        {
            if (_isDirty)
            {
                int begin = 0;
                foreach (VerticalIndexNode<T> item in _verticalIndex)
                {
                    item.BeginIndex = begin;
                    begin += item.List.Count;
                }
                _isDirty = false;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
