using System;
using System.Collections.ObjectModel;

namespace MoveShapeDemo
{
    public class KeyedCollectionEx<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private readonly Func<TItem, TKey> _getKeyForItemDelegate;
        public KeyedCollectionEx(Func<TItem, TKey> getKeyForItemDelegate) : base()
        {
            if (getKeyForItemDelegate == null)
                throw new ArgumentNullException("Delegate passed can't be null!");

            _getKeyForItemDelegate = getKeyForItemDelegate;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return _getKeyForItemDelegate(item);
        }
    }
}