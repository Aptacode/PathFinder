using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aptacode.PathFinder.Utilities
{
    public class PriorityQueue<TPriority, TItem>
    {
        private readonly List<TItem> _all;
        private readonly SortedDictionary<TPriority, List<TItem>> _storage;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<TPriority, List<TItem>>();
            _all = new List<TItem>();
        }

        public bool IsEmpty()
        {
            return _all.Count == 0;
        }

        public TItem Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }

            foreach (var q in _storage.Values.Where(q => q.Count > 0))
            {
                var item = q.First();
                q.Remove(item);
                _all.Remove(item);
                return item;
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default; // not supposed to reach here.
        }

        // same as above, except for peek.

        public TItem Peek()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            }

            foreach (var q in _storage.Values.Where(q => q.Count > 0))
            {
                return q.First();
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default; // not supposed to reach here.
        }

        public TItem Dequeue(TPriority priority)
        {
            var item = _storage[priority].First();
            _storage[priority].Remove(item);
            _all.Remove(item);
            return item;
        }

        public void Enqueue(TItem item, TPriority priority)
        {
            if (!_storage.ContainsKey(priority))
            {
                _storage.Add(priority, new List<TItem>());
            }

            _storage[priority].Add(item);
            _all.Add(item);
        }

        public bool Remove(TItem item, TPriority priority)
        {
            if (!_storage[priority].Remove(item))
            {
                return false;
            }

            _all.Remove(item);
            return true;
        }

        public IEnumerable<TItem> GetAll()
        {
            return _all;
        }

        public void Clear()
        {
            _storage.Clear();
            _all.Clear();
        }
    }
}