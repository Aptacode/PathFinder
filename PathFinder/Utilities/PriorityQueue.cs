using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aptacode.PathFinder.Utilities
{
    public class PriorityQueue<TPriority, TItem> where TItem : IEquatable<TItem>
    {
        private readonly SortedDictionary<TPriority, List<TItem>> _storage;
        private int _itemCount;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<TPriority, List<TItem>>();
            _itemCount = 0;
        }

        public bool IsEmpty()
        {
            return _itemCount == 0;
        }

        public TItem Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }

            var (key, value) = _storage.ElementAt(0);
            var element = value[0];
            value.RemoveAt(0);
            if (value.Count == 0)
            {
                _storage.Remove(key);
            }

            _itemCount--;
            return element;
        }

        // same as above, except for peek.

        public TItem Peek()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            }

            var (key, value) = _storage.ElementAt(0);
            if (value.Count > 0)
            {
                return value[0];
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default; // not supposed to reach here.
        }

        public TItem Dequeue(TPriority priority)
        {
            if (_storage.TryGetValue(priority, out var items))
            {
                var element = items[0];
                items.RemoveAt(0);
                if (items.Count == 0)
                {
                    _storage.Remove(priority);
                }

                _itemCount--;
                return element;
            }

            return default;
        }

        public void Enqueue(TItem item, TPriority priority)
        {
            if (!_storage.TryGetValue(priority, out var items))
            {
                items = new List<TItem>();
                _storage.Add(priority, items);
            }

            items.Add(item);
            _itemCount++;
        }

        public bool Remove(TItem item, TPriority priority)
        {
            if (_storage.TryGetValue(priority, out var items))
            {
                if (items.Remove(item))
                {
                    if (items.Count == 0)
                    {
                        _storage.Remove(priority);
                    }

                    _itemCount--;
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _storage.Clear();
            _itemCount = 0;
        }
    }
}