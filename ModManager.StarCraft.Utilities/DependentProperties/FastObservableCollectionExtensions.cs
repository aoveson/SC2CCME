using System;
using System.Collections.Generic;
using System.Linq;

namespace ModManager.StarCraft.Utilities.DependentProperties;

public static class FastObservableCollectionExtensions
{
    /// <summary>
    /// Updates the target FastObservableCollection to reflect the items in the source collection.
    /// For each item in the source collection, the corresponding item in the target collection is updated using the provided updateItem action.
    /// If the source collection has more items than the target collection, new items are created and added to the target collection. 
    /// If the source collection has fewer items than the target collection, the extra items in the target collection are removed.
    /// This method is useful for keeping a FastObservableCollection in sync with another collection, 
    /// such as when the source collection is updated and the target collection needs to reflect those changes. 
    /// The updateItem action is used to update the properties of the target items based on the corresponding source items.
    /// </summary>
    /// <typeparam name="T">The type of items in the target collection.</typeparam>
    /// <typeparam name="S">The type of items in the source collection.</typeparam>
    /// <param name="target">The target collection to update.</param>
    /// <param name="source">The source collection to reflect.</param>
    /// <param name="updateItem">The action to update each target item with its corresponding source item.</param>
    public static void Reflect<T, S>(this FastObservableCollection<T> target, IEnumerable<S> source, Action<T, S> updateItem) where T : new()
    {
        if (source != null)
        {
            var sourceCount = source.Count();
            int updateCount = Math.Min(sourceCount, target.Count());
            int newCount = sourceCount - updateCount;
            T[] newItems = new T[newCount];
            int index = 0;

            foreach (S sourceItem in source)
            {
                T targetItem;
                if (index < updateCount)
                {
                    // Update properties of pre-existing items in target collection.
                    targetItem = target[index];
                }
                else
                {
                    // Add new items so target collection until it is the same size as source, if source is larger.
                    targetItem = new T();
                    newItems[index - updateCount] = targetItem;
                }

                updateItem(targetItem, sourceItem);
                ++index;
            }

            if (newItems.Length > 0)
            {
                target.AddRange(newItems);
            }
            else if (target.Count > sourceCount)
            {
                // Delete extra items so target collection is the same size as source.
                target.Truncate(sourceCount);
            }
        }
        else
        {
            target.Clear();
        }
    }
}
