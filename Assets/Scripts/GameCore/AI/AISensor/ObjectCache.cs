using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SAISensor
{
    public class ObjectCache<T>
    {
        private Stack<T> cache;

        public ObjectCache(int startSize)
        {
            cache = new Stack<T>();
            for (int i = 0; i < startSize; i++)
            {
                cache.Push(create());
            }
        }

        public ObjectCache() : this(10) { }
  
        public T Get()
        {
            if (cache.Count > 0)
                return cache.Pop();
            else
                return create();
        }

        /// <summary>
        /// 回收，循环利用缓存
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Cycle(T obj)
        {
            cache.Push(obj);
        }

        protected virtual T create()
        {
            return System.Activator.CreateInstance<T>();
        }
    }

    public class ListCache<T> : ObjectCache<List<T>>
    {
        public override void Cycle(List<T> obj)
        {
            obj.Clear();
            base.Cycle(obj);
        }
    }

}