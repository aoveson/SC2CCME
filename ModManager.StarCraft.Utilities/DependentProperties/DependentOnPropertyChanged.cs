using System;
using System.ComponentModel;
using System.Linq;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
internal class DependentOnPropertyChanged<T> : IDependent<T>
{
    /// <summary>
    /// The function that computes the value of the dependent property based on other properties. 
    /// This function is called whenever a source property changes.
    /// </summary>
    private readonly Func<T> _Function;

    /// <summary>
    /// The action to be executed when the dependent property value changes. 
    /// This action is called after the value is updated and can be used to notify subscribers or perform additional logic.
    /// </summary>
    private readonly Action _OnChanged;

    /// <summary>
    /// Initializes a new instance of the DependentOnPropertyChanged class with the specified function and onChanged action.
    /// The function computes the value of the dependent property, and the onChanged action is executed whenever the value changes. 
    /// The initial value of the dependent property is computed by invoking the function.
    /// </summary>
    /// <param name="function">The function that computes the value of the dependent property</param>
    /// <param name="onChanged">The action to be executed when the dependent property value changes</param>
    public DependentOnPropertyChanged(Func<T> function, Action onChanged)
    {
        _Function = function;
        _OnChanged = onChanged;
        Value = _Function();
    }

    /// <summary>
    /// Gets the current value of the dependent property. 
    /// This value is automatically updated whenever a source property changes and the function is invoked to compute the new value.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Refreshes the value of the dependent property by invoking the function and updating the Value property.
    /// If the new value is different from the current value, the Value property is updated and the OnChanged action is executed. 
    /// This method can be called manually to force a refresh of the dependent property value, for example, when a source property changes.
    /// </summary>
    public void Refresh()
    {
        var newValue = _Function();
        if (!Equals(newValue, Value))
        {
            ApplyChangeAndNotify(newValue);
        }
    }

    /// <summary>
    /// Applies the new value to the Value property and executes the OnChanged action.
    /// This method is called internally when the value of the dependent property changes, 
    /// ensuring that the Value property is updated and any subscribers are notified of the change. 
    /// It is not intended to be called directly from outside the class, 
    /// as the Refresh method handles the logic for determining when to update the value and notify subscribers.
    /// </summary>
    /// <param name="newValue">The new value to apply</param>
    protected virtual void ApplyChangeAndNotify(T newValue)
    {
        Value = newValue;
        _OnChanged();
    }

    /// <summary>
    /// Refreshes the value of the dependent property when a source property changes.
    /// This method is intended to be used as an event handler for the PropertyChanged event of source properties. 
    /// When a source property changes, this method is called, 
    /// which in turn calls the Refresh method to update the value of the dependent property. 
    /// It can be used to automatically refresh the dependent property whenever any of its source properties change, 
    /// ensuring that the dependent property always reflects the current state of its dependencies.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments</typeparam>
    /// <param name="s">The source of the event</param>
    /// <param name="e">The event arguments</param>
    public void Refresh<TEventArgs>(object s, TEventArgs e) => Refresh();

    /// <summary>
    /// Creates a PropertyChangedEventHandler that refreshes the dependent property when any of the specified source properties change.
    /// This method allows you to specify a list of source property names that the dependent property depends on. 
    /// When any of those source properties change, the returned event handler will be invoked, 
    /// which in turn calls the Refresh method to update the value of the dependent property. 
    /// </summary>
    /// <param name="sourceProperties">The list of source property names that the dependent property depends on</param>
    /// <returns>The PropertyChangedEventHandler</returns>
    public PropertyChangedEventHandler RefreshFor(params string[] sourceProperties)
    {
        return (s, e) =>
        {
            if (sourceProperties.Contains(e.PropertyName))
            {
                Refresh();
            }
        };
    }
}

