using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ActiveCache.App
{
    /// <summary>Очередь с приоритетом элементов</summary>
    public class PriorityQueue<T, TKey> : IEnumerable<T>
        where TKey : IComparable
		where T: class
    {
        private Func<T, TKey> keySelector;
        private List<T> heap;
        private bool ascending;

        /// <summary>Количество элементов в очереди</summary>
        public int Count { get { return heap.Count; } }

        /// <summary>
        /// Очередь с приоритетом элементов
        /// </summary>
        /// <param name="keySelector">Кл</param>
        /// <param name="ascending"></param>
        public PriorityQueue(Func<T, TKey> key, bool ascending = false)
        {
            heap = new List<T>();
            this.keySelector = key;
            this.ascending = ascending;
        }

        /// <summary>Заталкивает элемент в очередь</summary>
        public void Enqueue(T item)
        {
            lock (heap)
            {
                heap.Add(item);
                PropagateUp(heap.Count);
            }
        }

        /// <summary>Извлекает элемент из очереди (с наивысшим приоритетом)</summary>
        public T Dequeue()
        {
			if (IsEmpty())
				return null;

            lock (heap)
            {
                var max = AtIndex(1);
                Exchange(1, Count);
                heap.RemoveAt(Count - 1);
                PropagateDown(1);
                return max;
            }
        }

        public T Top()
        {
			if (IsEmpty())
				return null;

            lock(heap)
                return AtIndex(1);
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }


		public int Size()
		{
			lock (heap)
				return heap.Count;
		}


        private void PropagateUp(int k)
        {
            while (k > 1 && Less(AtIndex(k / 2), AtIndex(k)))
            {
                Exchange(k / 2, k);
                k = k / 2;
            }
        }

        private void PropagateDown(int k)
        {
            while (2 * k <= Count)
            {
                int j = 2 * k;

                if (j < Count && Less(AtIndex(j), AtIndex(j + 1))) 
                    j++;

                if (!Less(AtIndex(k), AtIndex(j))) 
                    break;

                Exchange(k, j);
                k = j;
            }
        }

        private T AtIndex(int index)
        {
            return heap[index - 1];
        }

        private bool Less(T a, T b)
        {
            var aKey = keySelector(a);
            var bKey = keySelector(b);

            var order = aKey.CompareTo(bKey);

            return ascending
                ? order > 0
                : order < 0;
        }

        private void Exchange(int aIdx, int bIdx)
        {
            if (aIdx == bIdx)
                return;

            var temp = heap[aIdx - 1];
            heap[aIdx - 1] = heap[bIdx - 1];
            heap[bIdx - 1] = temp;
        }


        #region IEnumerable members
        public IEnumerator<T> GetEnumerator()
        {
            IList<T> result = Dump();
            return result.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            System.Collections.IEnumerable result = Dump();
            return result.GetEnumerator();
        }

        private IList<T> Dump()
        {
            lock (heap)
            {
                return ascending
                    ? heap.OrderBy(keySelector).ToList()
                    : heap.OrderByDescending(keySelector).ToList();
            }
        }

        #endregion
    }

}
