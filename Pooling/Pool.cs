// // *************************************************************************** 
// // This is free and unencumbered software released into the public domain.
// // 
// // Anyone is free to copy, modify, publish, use, compile, sell, or
// // distribute this software, either in source code form or as a compiled
// // binary, for any purpose, commercial or non-commercial, and by any
// // means.
// // 
// // In jurisdictions that recognize copyright laws, the author or authors
// // of this software dedicate any and all copyright interest in the
// // software to the public domain. We make this dedication for the benefit
// // of the public at large and to the detriment of our heirs and
// // successors. We intend this dedication to be an overt act of
// // relinquishment in perpetuity of all present and future rights to this
// // software under copyright law.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// // IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// // OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// // ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// // OTHER DEALINGS IN THE SOFTWARE.
// // 
// // For more information, please refer to <http://unlicense.org>
// // ***************************************************************************

using System;
using System.Collections.Concurrent;

namespace Pooling
{
    public class PoolEventArgs<T> : EventArgs
    {
        public T Object { get; set; }

        public PoolEventArgs(T createdObject)
        {
            Object = createdObject;
        }
    }

    public interface PoolItem
    {
        /// <summary>
        ///     This method will be called by the pool upon reuse of this item.
        /// </summary>
        void PoolReuse();

        /// <summary>
        ///     This method will be called by the pool upon creation of this item.
        /// </summary>
        void PoolCreate();

        /// <summary>
        ///     This method will be called by the pool upon return of this item.
        /// </summary>
        void PoolReturn();
    }

    /// <summary>
    ///     This class is a lock-free object-pool implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : class, PoolItem
    {
        public long CreationCount { get; set; }
        public event EventHandler<PoolEventArgs<T>> Created;
        public event EventHandler<PoolEventArgs<T>> Reused;
        public event EventHandler<PoolEventArgs<T>> Returned;

        private ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
        private readonly object[] constructorParameters;

        public Pool()
        {
            constructorParameters = new object[] {};
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pool{T}" /> class.
        /// </summary>
        /// <param name="constructorParameters">The constructor parameters of the to-be-constructed pool-items.</param>
        public Pool(object[] constructorParameters)
        {
            this.constructorParameters = constructorParameters;
        }

        /// <summary>
        ///     Gets the next item from the pool or constructs one if the pool is empty.
        /// </summary>
        /// <returns>An Item.</returns>
        public T Get()
        {
            T result;
            if (!pool.TryDequeue(out result))
            {
                // The queue was empty. Construct new T.
                result = (T) Activator.CreateInstance(typeof(T), constructorParameters);
                result.PoolCreate();
                Created?.Invoke(this, new PoolEventArgs<T>(result));
                CreationCount++;
            }
            else
            {
                result.PoolReuse();
                Reused?.Invoke(this, new PoolEventArgs<T>(result));
            }
            return result;
        }

        /// <summary>
        ///     Return an item back to the pool.
        /// </summary>
        /// <param name="item">The item to return</param>
        /// <returns>This instance in order to support a fluent interface.</returns>
        public Pool<T> Return(T item)
        {
            item.PoolReturn();
            pool.Enqueue(item);
            Returned?.Invoke(this, new PoolEventArgs<T>(item));
            return this;
        }

        /// <summary>
        ///     Gets the number of currently enqueued items.
        /// </summary>
        /// <returns>A number.</returns>
        public int Count()
        {
            return pool.Count;
        }

        public Pool<T> Clear()
        {
            T item;
            while (pool.TryDequeue(out item))
            {}
            return this;
        }

        public void Dispose()
        {
            Clear();
            Created = null;
            Reused = null;
            Returned = null;
            pool = null;
        }
    }
}