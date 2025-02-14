using System;
using System.Collections.Generic;

/*
 * FloatHOTQueue<TValue>
 * ----------------------
 * A priority-based queue implementation that splits elements into two separate queues: a "hot queue" and a "cold queue."
 * This design provides an efficient way to process high-priority elements while maintaining lower-priority elements for later handling.
 *
 * Key Features:
 * -------------
 * 1. Dual-Queue System:
 *    - Hot Queue: Stores elements with higher priorities (lower float values). Elements in this queue are processed first.
 *    - Cold Queue: Stores elements with lower priorities (higher float values). This queue serves as a buffer and is converted 
 *      into the hot queue when the hot queue is empty.
 * 
 * 2. Threshold Management:
 *    - `hotThreshold` tracks the highest priority (smallest float value) in the hot queue.
 *    - `coldThreshold` tracks the lowest priority (largest float value) in the cold queue.
 *    - These thresholds are updated dynamically as elements are enqueued or dequeued.
 *
 * 3. Efficient Heap-Based Implementation:
 *    - Both hot and cold queues utilize a binary heap for efficient insertion and extraction operations.
 *    - The underlying `PriorityQueue` class maintains heap properties to ensure optimal performance.
 *
 * Supported Operations:
 * ----------------------
 * - `Enqueue(float priority, TValue value)`: Adds an element to the appropriate queue based on its priority.
 * - `Dequeue()`: Removes and returns the element with the highest priority from the hot queue.
 * - `Peek()`: Returns the element with the highest priority without removing it from the hot queue.
 * - `Clear()`: Empties both queues and resets all thresholds.
 * - `Count`: Retrieves the total number of elements across both queues.
 *
 * Design Considerations:
 * -----------------------
 * - The system prioritizes processing elements in the hot queue while maintaining a secondary cold queue to 
 *   delay processing of lower-priority elements.
 * - When the hot queue is empty, the cold queue is seamlessly converted into the hot queue, reducing the need 
 *   for frequent priority adjustments.
 *
 * Use Cases:
 * ----------
 * This data structure is ideal for scenarios where:
 * - Tasks or resources need to be managed and processed in priority order.
 * - Workloads are divided into smaller, high-priority tasks and larger, low-priority tasks.
 * - Efficiency is critical, especially when handling large numbers of elements with varying priority levels.
 *
 * Example:
 * --------
 * var queue = new FloatHOTQueue<string>();
 * queue.Enqueue(1.0f, "High Priority Task");
 * queue.Enqueue(10.0f, "Low Priority Task");
 * var nextTask = queue.Dequeue(); // Returns "High Priority Task"
 */

namespace ZeroPass
{
    public class FloatHOTQueue<TValue>
    {
        private class PriorityQueue
        {
            private List<KeyValuePair<float, TValue>> _baseHeap;

            public int Count => _baseHeap.Count;

            public PriorityQueue()
            {
                _baseHeap = new List<KeyValuePair<float, TValue>>();
            }

            public void Enqueue(float priority, TValue value)
            {
                Insert(priority, value);
            }

            public KeyValuePair<float, TValue> Dequeue()
            {
                KeyValuePair<float, TValue> result = _baseHeap[0];
                DeleteRoot();
                return result;
            }

            public KeyValuePair<float, TValue> Peek()
            {
                if (Count > 0)
                {
                    return _baseHeap[0];
                }
                throw new InvalidOperationException("Priority queue is empty");
            }

            private void ExchangeElements(int pos1, int pos2)
            {
                KeyValuePair<float, TValue> value = _baseHeap[pos1];
                _baseHeap[pos1] = _baseHeap[pos2];
                _baseHeap[pos2] = value;
            }

            private void Insert(float priority, TValue value)
            {
                KeyValuePair<float, TValue> item = new KeyValuePair<float, TValue>(priority, value);
                _baseHeap.Add(item);
                HeapifyFromEndToBeginning(_baseHeap.Count - 1);
            }

            private int HeapifyFromEndToBeginning(int pos)
            {
                if (pos >= _baseHeap.Count)
                {
                    return -1;
                }
                while (pos > 0)
                {
                    int num = (pos - 1) / 2;
                    if (!(_baseHeap[num].Key - _baseHeap[pos].Key > 0f))
                    {
                        break;
                    }
                    ExchangeElements(num, pos);
                    pos = num;
                }
                return pos;
            }

            private void DeleteRoot()
            {
                if (_baseHeap.Count <= 1)
                {
                    _baseHeap.Clear();
                }
                else
                {
                    _baseHeap[0] = _baseHeap[_baseHeap.Count - 1];
                    _baseHeap.RemoveAt(_baseHeap.Count - 1);
                    HeapifyFromBeginningToEnd(0);
                }
            }

            private void HeapifyFromBeginningToEnd(int pos)
            {
                int count = _baseHeap.Count;
                if (pos < count)
                {
                    while (true)
                    {
                        int num = pos;
                        int num2 = 2 * pos + 1;
                        int num3 = 2 * pos + 2;
                        if (num2 < count && _baseHeap[num].Key - _baseHeap[num2].Key > 0f)
                        {
                            num = num2;
                        }
                        if (num3 < count && _baseHeap[num].Key - _baseHeap[num3].Key > 0f)
                        {
                            num = num3;
                        }
                        if (num == pos)
                        {
                            break;
                        }
                        ExchangeElements(num, pos);
                        pos = num;
                    }
                }
            }

            public void Clear()
            {
                _baseHeap.Clear();
            }
        }

        private PriorityQueue hotQueue = new PriorityQueue();

        private PriorityQueue coldQueue = new PriorityQueue();

        private float hotThreshold = -3.40282347E+38f;

        private float coldThreshold = -3.40282347E+38f;

        private int count;

        public int Count => count;

        public KeyValuePair<float, TValue> Dequeue()
        {
            if (hotQueue.Count == 0)
            {
                PriorityQueue priorityQueue = hotQueue;
                hotQueue = coldQueue;
                coldQueue = priorityQueue;
                hotThreshold = coldThreshold;
            }
            count--;
            return hotQueue.Dequeue();
        }

        public void Enqueue(float priority, TValue value)
        {
            if (priority <= hotThreshold)
            {
                hotQueue.Enqueue(priority, value);
            }
            else
            {
                coldQueue.Enqueue(priority, value);
                coldThreshold = Math.Max(coldThreshold, priority);
            }
            count++;
        }

        public KeyValuePair<float, TValue> Peek()
        {
            if (hotQueue.Count == 0)
            {
                PriorityQueue priorityQueue = hotQueue;
                hotQueue = coldQueue;
                coldQueue = priorityQueue;
                hotThreshold = coldThreshold;
            }
            return hotQueue.Peek();
        }

        public void Clear()
        {
            count = 0;
            hotThreshold = -3.40282347E+38f;
            hotQueue.Clear();
            coldThreshold = -3.40282347E+38f;
            coldQueue.Clear();
        }
    }
}