using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aptacode.PathFinder
{
    public class PriorityQueue<TPriority, TItem>
    {
        private readonly SortedDictionary<TPriority, Queue<TItem>> _storage;

        private int _totalSize;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<TPriority, Queue<TItem>>();
            _totalSize = 0;
        }

        public bool IsEmpty() => _totalSize == 0;

        public TItem Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }

            foreach (var q in _storage.Values.Where(q => q.Count > 0))
            {
                _totalSize--;
                return q.Dequeue();
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
                return q.Peek();
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default; // not supposed to reach here.
        }

        public TItem Dequeue(TPriority priority)
        {
            _totalSize--;
            return _storage[priority].Dequeue();
        }

        public void Enqueue(TItem item, TPriority priority)
        {
            if (!_storage.ContainsKey(priority))
            {
                _storage.Add(priority, new Queue<TItem>());
            }

            _storage[priority].Enqueue(item);
            _totalSize++;
        }
    }
}