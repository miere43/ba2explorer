namespace Ba2Explorer.Utility
{
    public abstract class Poolable
    {
        /// <summary>
        /// Indicates whether this object is marked as 'free' in it's ObjectPool.
        /// </summary>
        internal bool m_free;

        /// <summary>
        /// Resets all meaningful fields to their defaults.
        /// </summary>
        public abstract void Reset();
    }
}