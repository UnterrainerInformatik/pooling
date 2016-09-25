// *************************************************************************** 
//  Copyright (c) 2015 by Unterrainer Informatik OG.
//  This source is licensed to Unterrainer Informatik OG.
//  All rights reserved.
//  
//  In other words:
//  YOU MUST NOT COPY, USE, CHANGE OR REDISTRIBUTE ANY ART, MUSIC, CODE OR
//  OTHER DATA, CONTAINED WITHIN THESE DIRECTORIES WITHOUT THE EXPRESS
//  PERMISSION OF Unterrainer Informatik OG.
// ---------------------------------------------------------------------------
//  Programmer: G U, 
//  Created: 2015-09-09
// ***************************************************************************

using System;
using System.Collections.Concurrent;

namespace Pooling
{
    public class PoolEventArgs<T> : EventArgs
    {
        public T Item { get; set; }

        public PoolEventArgs(T pooledObject)
        {
            Item = pooledObject;
        }
    }

    /// <summary>
    ///     This class is a lock-free pool implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : class
    {
        public long CreationCount { get; set; }
        public event EventHandler<PoolEventArgs<T>> Created;
        public event EventHandler<PoolEventArgs<T>> Reused;
        public event EventHandler<PoolEventArgs<T>> Returned;

        private ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
        private readonly object[] constructorParameters;

        public Pool()
        {
            constructorParameters = new object[] { };
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
                result = (T)Activator.CreateInstance(typeof(T), constructorParameters);
                Created?.Invoke(this, new PoolEventArgs<T>(result));
                CreationCount++;
            }
            else
            {
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

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        /// <returns>This instance in order to support a fluent interface.</returns>
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