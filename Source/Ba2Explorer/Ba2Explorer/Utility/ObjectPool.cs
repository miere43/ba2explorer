using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.View;

namespace Ba2Explorer.Utility
{
    public class ObjectPool<T> where T: Poolable, new()
    {
        private T[] m_pool;

        private const float growRatio = 1.7f;

        private int m_prevFreeItemIndex = 0;

        private int m_prevReturnItemIndex = 0;

        private int m_numUsed = 0;

        private void Grow(int size)
        {
            Array.Resize(ref m_pool, m_pool.Length + size);
        }

        public ObjectPool(int initial)
        {
            Debug.Assert(initial >= 10);
            m_pool = new T[initial];
        }

        // public void Compact(); // removes gaps between m_pool items
        // public void Shrink(); // shrinks m_pool array to size of m_numUsed (doesn't call Compact())

        private int LocateFreeCell()
        {
            int freeIndex = -1;
            if (m_numUsed == m_pool.Length)
            {
                freeIndex = m_pool.Length;
                Grow((int)(m_pool.Length * growRatio - m_pool.Length));
                return freeIndex;
            }

            int numIterated = 0;
            for (int i = m_prevFreeItemIndex; numIterated < m_pool.Length; i = (i + 1) % m_pool.Length)
            {
                if (m_pool[i] == null || m_pool[i].m_free)
                {
                    freeIndex = i;
                    break;
                }
                ++numIterated;
            }

            return freeIndex;
        }

        private int LocateObject(T obj)
        {
            int iterated = 0;
            for (int i = m_prevReturnItemIndex; iterated < m_pool.Length; i = (i + 1) % m_pool.Length)
            {
                if (obj.Equals(m_pool[i]) /* no null check nessessary */)
                {
                    m_prevReturnItemIndex = i;
                    return i;
                }
                ++iterated;
            }
            return -1;
        }

        /// <summary>
        /// Take object from pool.
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            int i = LocateFreeCell();
            T obj = m_pool[i];
            if (obj == null)
            {
                obj = new T();
                obj.Reset();
                m_pool[i] = obj;
            }

            m_prevFreeItemIndex = i;
            ++m_numUsed;
            obj.m_free = false;
            return obj;
        }

        public void Return(T obj)
        {
            int pos = LocateObject(obj);
            Debug.Assert(pos != -1);
            m_pool[pos].Reset();
            m_pool[pos].m_free = true;
            --m_numUsed;
        }

        /// <summary>
        /// Grows pool if there are no free space for at least 'items' items.
        /// </summary>
        /// <param name="items"></param>
        public void GrowToContain(int items)
        {
            int numFree = m_pool.Length - m_numUsed;
            if (numFree < items) Grow(items - numFree);
        }

        public void ResetItemPointers()
        {
            m_prevReturnItemIndex = 0;
            m_prevFreeItemIndex = 0;
        }
    }
}
