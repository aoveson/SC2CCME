using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// An ObservableCollection that provides methods for adding and removing multiple items at once, 
/// and raises the appropriate collection changed events.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class FastObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Initializes a new instance of the FastObservableCollection class that is empty and has default initial capacity.
    /// </summary>
    public FastObservableCollection() : base() { }

    /// <summary>
    /// Initializes a new instance of the FastObservableCollection class that contains elements copied 
    /// from the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    public FastObservableCollection(IEnumerable<T> collection) : base(collection) { }

    /// <summary>
    /// Adds a range of items to the collection and raises the appropriate collection changed events.
    /// If more than one item is added, a Reset event is raised to indicate that the entire collection has changed. 
    /// If only one item is added, an Add event is raised for that item. 
    /// This method is useful for adding multiple items to the collection at once without raising multiple collection changed events, 
    /// which can improve performance and reduce UI updates.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the items parameter is null.</exception>
    public void AddRange(IList<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (items.Count > 0)
        {
            CheckReentrancy();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            if (items.Count > 1)
            {
                // List changed significantly. Reset entire list.
                NotifyChange(NotifyCollectionChangedAction.Reset);
            }
            else
            {
                NotifyChange(NotifyCollectionChangedAction.Add, items.First());
            }
        }
    }
    
    /// <summary>
    /// Removes all items from the collection that are not present in the specified list of items, and raises the appropriate collection changed events.
    /// If more than one item is removed, a Reset event is raised to indicate that the entire collection has changed. 
    /// If only one item is removed, a Remove event is raised for that item. 
    /// This method is useful for removing multiple items from the collection at once without raising multiple collection changed events, 
    /// which can improve performance and reduce UI updates.
    /// </summary>
    /// <param name="count">The number of items to keep in the collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the count parameter is negative.</exception>
    public void Truncate(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (Count > count)
        {
            CheckReentrancy();
            var itemsToDelete = this.Skip(count).ToList();
            if (count == 0)
            {
                Items.Clear();
            }
            else
            {
                for (int i = Items.Count - 1; i >= count; i--)
                {
                    Items.RemoveAt(i);
                }
            }

            if (itemsToDelete.Count > 1)
            {
                // List changed significantly. Reset entire list.
                NotifyChange(NotifyCollectionChangedAction.Reset);
            }
            else
            {
                NotifyChange(NotifyCollectionChangedAction.Remove, itemsToDelete.First());
            }
        }
    }

    /// <summary>
    /// Raises the CollectionChanged event with the specified action and item, 
    /// and raises the PropertyChanged events for "Count" and "Item[]".
    /// This method is used to notify subscribers that the collection has changed, 
    /// and to update any UI elements that are bound to the collection's Count or Item[] properties.
    /// </summary>
    /// <param name="action">The action that caused the change.</param>
    /// <param name="item">The item that was changed.</param>
    private void NotifyChange(NotifyCollectionChangedAction action, object item = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
    }
}
