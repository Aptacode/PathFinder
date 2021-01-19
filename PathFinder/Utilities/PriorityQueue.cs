using System;
using System.Collections.Generic;
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
            var (key, value) = _storage.First();
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
            return _storage.Values.First()[0];
        }

        public TItem Dequeue(TPriority priority)
        {
            if (!_storage.TryGetValue(priority, out var items))
            {
                return default;
            }

            var element = items[0];
            items.RemoveAt(0);
            if (items.Count == 0)
            {
                _storage.Remove(priority);
            }

            _itemCount--;
            return element;
        }

        public void Enqueue(TItem item, TPriority priority)
        {
            if (_storage.TryGetValue(priority, out var items))
            {
                items.Add(item);
            }
            else
            {
                _storage.Add(priority, new List<TItem> {item});
            }

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