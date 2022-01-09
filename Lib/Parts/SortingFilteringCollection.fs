module SortingFilteringCollection
open System
open System.Collections

// wip porting of https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Game.cs#L940

type AddJournalEntry(order: int, item: T) =
    struct
        let mutable Order = order
        let Item = item

        static member CreateKey(item: T ) =
            AddJournalEntry(-1, item)

        override this.GetHashCode() =
            Item.GetHashCode()

        override this.Equals(obj: Object) =
            if (!(obj is AddJournalEntry<T>)) then
                false
            else
                let object = this :> Object
                object.Equals(Item, ((AddJournalEntry<T>)obj).Item)
    end

type SortingFilteringCollection (
    filter: Predicate<T>,
    filterChangedSubscriber: Action<T, EventHandler<EventArgs>> ,
    filterChangedUnsubscriber: Action<T, EventHandler<EventArgs>> ,
    sort: Comparison<T>,
    sortChangedSubscriber: Action<T, EventHandler<EventArgs>> ,
    sortChangedUnsubscriber: Action<T, EventHandler<EventArgs>> 
    ) =
    inherit ICollection<T>
        
    let _items = new Generic.List<T>()
    let _addJournal = new List<AddJournalEntry<T>>()
    let _removeJournal = new List<int>()
    let _cachedFilteredItems = new List<T>()
    let mutable _shouldRebuildCache = true

    let _filter = filter
    let _filterChangedSubscriber = filterChangedSubscriber
    let _filterChangedUnsubscriber = filterChangedUnsubscriber
    let _sort = sort
    let _sortChangedSubscriber = sortChangedSubscriber
    let _sortChangedUnsubscriber = sortChangedUnsubscriber

    let _addJournalSortComparison = CompareAddJournalEntry

    member this.CompareAddJournalEntry(x: AddJournalEntry<T>,y: AddJournalEntry<T>) =
    
        let result = _sort(x.Item, y.Item)
        if (result <> 0) then
            result
        else
            x.Order - y.Order
    

    member this.ForEachFilteredItem(action: Action<T, TUserData> , userData TUserData ) =
        if (_shouldRebuildCache) then
            ProcessRemoveJournal()
            ProcessAddJournal()

            // Rebuild the cache
            _cachedFilteredItems.Clear()
            for i in 0 .. _items.Count do
                if (_filter(_items[i])) then
                    _cachedFilteredItems.Add(_items[i])

            _shouldRebuildCache = false

        for i in 0 .. _cachedFilteredItems.Count do
            action(_cachedFilteredItems[i], userData);

        // If the cache was invalidated as a result of processing items,
        // now is a good time to clear it and give the GC (more of) a
        // chance to do its thing.
        if (_shouldRebuildCache) then
            _cachedFilteredItems.Clear()

    member this.Add( item: T) =
        // NOTE: We subscribe to item events after items in _addJournal
        //       have been merged.
        _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item))
        InvalidateCache()

    member this.Remove(item: T)=
        if (_addJournal.Remove(AddJournalEntry<T>.CreateKey(item))) then
            true
        else
            var index = _items.IndexOf(item)
            if (index >= 0) then
                UnsubscribeFromItemEvents(item)
                _removeJournal.Add(index)
                InvalidateCache()
                true
            else 
                false

    member this.Clear()=
        for i in 0 .. _items.Count do
            _filterChangedUnsubscriber(_items[i], Item_FilterPropertyChanged)
            _sortChangedUnsubscriber(_items[i], Item_SortPropertyChanged)

        _addJournal.Clear()
        _removeJournal.Clear()
        _items.Clear()

        InvalidateCache()

    member this.Contains(item: T) =
        _items.Contains(item)

    member this.CopyTo(array: T[] , arrayIndex: int) =
        _items.CopyTo(array, arrayIndex)

    member this.Count with get ()= _items.Count

    member this.IsReadOnly wiht get () = false

    member this.GetEnumerator() =_items.GetEnumerator()
    
    // member System.Collections.IEnumerable.GetEnumerator() =
    //     (System.Collections.IEnumerable _items).GetEnumerator();

    static member RemoveJournalSortComparison =
        fun (x, y) -> Comparer.Default.Compare(y, x) // Sort high to low
    // static readonly Comparison<int> RemoveJournalSortComparison =
    //     (x, y) => Comparer<int>.Default.Compare(y, x); // Sort high to low

    member this.ProcessRemoveJournal() =
        if (_removeJournal.Count == 0) then
            ()
        else

        // Remove items in reverse.  (Technically there exist faster
        // ways to bulk-remove from a variable-length array, but List<T>
        // does not provide such a method.)
            _removeJournal.Sort(RemoveJournalSortComparison)
            for i in 0 .. _removeJournal.Count do
                _items.RemoveAt(_removeJournal[i])
            _removeJournal.Clear()
    

    member this.ProcessAddJournal() =
        if (_addJournal.Count == 0) then
            ()
        else

        // Prepare the _addJournal to be merge-sorted with _items.
        // _items is already sorted (because it is always sorted).
            _addJournal.Sort(_addJournalSortComparison);

            let mutable iAddJournal = 0;
            let mutable iItems = 0;

            while (iItems < _items.Count && iAddJournal < _addJournal.Count) do
                var addJournalItem = _addJournal[iAddJournal].Item;
                // If addJournalItem is less than (belongs before)
                // _items[iItems], insert it.
                if (_sort(addJournalItem, _items[iItems]) < 0) then
                    SubscribeToItemEvents(addJournalItem);
                    _items.Insert(iItems, addJournalItem);
                    iAddJournal <- iAddJournal + 1
                // Always increment iItems, either because we inserted and
                // need to move past the insertion, or because we didn't
                // insert and need to consider the next element.
                iItems <- iItems + 1

            // If _addJournal had any "tail" items, append them all now.
            // for (; iAddJournal < _addJournal.Count; ++iAddJournal)
            while iAddJournal < _addJournal.Count do
                let addJournalItem = _addJournal[iAddJournal].Item
                SubscribeToItemEvents(addJournalItem)
                _items.Add(addJournalItem)
                iAddJournal = iAddJournal + 1

            _addJournal.Clear()

    member this.SubscribeToItemEvents(item: T ) =
        _filterChangedSubscriber(item, Item_FilterPropertyChanged)
        _sortChangedSubscriber(item, Item_SortPropertyChanged)

    member this.UnsubscribeFromItemEvents(item: T ) =
        _filterChangedUnsubscriber(item, Item_FilterPropertyChanged)
        _sortChangedUnsubscriber(item, Item_SortPropertyChanged)

    member this.InvalidateCache() =
        _shouldRebuildCache <- true

    member this.Item_FilterPropertyChanged(sender: object , e: EventArgs ) =
        InvalidateCache();

    member this.Item_SortPropertyChanged(sender: object , e: EventArgs) =
        var item = T sender
        var index = _items.IndexOf(item)

        _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item))
        _removeJournal.Add(index)

        // Until the item is back in place, we don't care about its
        // events.  We will re-subscribe when _addJournal is processed.

        UnsubscribeFromItemEvents(item)
        InvalidateCache()
