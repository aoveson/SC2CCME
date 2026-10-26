using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// Extension methods for IDependentNeedsReturns to support returning collections of items based on a source list and an update action.
/// </summary>
public static class DependentNeedsReturnsExtensions
{
    /// <summary>
    /// Returns a collection of items that are created and updated based on a source list and an update action. 
    /// The collection is automatically updated when the source list changes.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the collection</typeparam>
    /// <typeparam name="TSource">The type of items in the source list</typeparam>
    /// <param name="configure">The configuration object for the dependent property</param>
    /// <param name="sourceList">A function that returns the source list of items</param>
    /// <param name="updateItem">An action that updates an item with the values from the source list</param>
    /// <returns>A configuration object for the dependent property</returns>
    public static IDependentNeedsChangesWith<ObservableCollection<TItem>> ReturnsCollection<TItem, TSource>(
        this IDependentNeedsReturns<ObservableCollection<TItem>> configure,
        Func<IReadOnlyList<TSource>> sourceList,
        Action<TItem, TSource> updateItem)
        where TItem : new()
    {
        FastObservableCollection<TItem> _itemsList = null;
        int _itemsListThread = -1;
        return configure.Returns(() => {
            var sourceItems = sourceList();
            if (_itemsList == null || _itemsListThread != Thread.CurrentThread.ManagedThreadId)
            {
                _itemsList = new FastObservableCollection<TItem>(sourceItems.Select(s =>
                {
                    var t = new TItem();
                    updateItem(t, s);
                    return t;
                }));
                _itemsListThread = Thread.CurrentThread.ManagedThreadId;
            }
            else
            {
                _itemsList.Reflect(sourceItems, updateItem);
            }

            return _itemsList;
        });
    }
}
