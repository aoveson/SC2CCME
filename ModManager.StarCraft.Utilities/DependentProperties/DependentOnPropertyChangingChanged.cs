using System;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// A dependent property that updates its value when the specified property changes, and also subscribes to events on that property.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
internal class DependentOnPropertyChangingChanged<T> : DependentOnPropertyChanged<T>
{
    /// <summary>
    /// The action to be executed when the dependent property value is about to change.
    /// </summary>
    private readonly Action _OnChanging;

    /// <summary>
    /// Initializes a new instance of the DependentOnPropertyChangingChanged class with the specified function, 
    /// OnChanged action, and OnChanging action.
    /// </summary>
    /// <param name="function">The function to compute the dependent property's value</param>
    /// <param name="onChanged">The action to be executed when the dependent property value changes</param>
    /// <param name="onChanging">The action to be executed when the dependent property value is about to change</param>
    public DependentOnPropertyChangingChanged(Func<T> function, Action onChanged, Action onChanging)
        : base(function, onChanged)
    {
        _OnChanging = onChanging;
    }

    /// <summary>
    /// Applies the new value to the dependent property and executes the OnChanging action before notifying subscribers
    /// of the change. This ensures that any necessary logic can be performed before the value is
    /// updated and subscribers are notified.
    /// </summary>
    /// <param name="newValue">The new value to be applied to the dependent property</param>
    protected override void ApplyChangeAndNotify(T newValue)
    {
        _OnChanging();
        base.ApplyChangeAndNotify(newValue);
    }
}

