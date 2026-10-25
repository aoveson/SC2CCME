using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// This dependent needs to be configured with its dependencies and update logic before it can be used.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
public interface IDependentNeedsModel<T>
{
    /// <summary>
    /// Configures the dependent property with the specified model and update logic.
    /// The model is used to subscribe to property change notifications, and the function is used to 
    /// compute the value of the dependent property based on the current state of the model.
    /// </summary>
    /// <param name="model">The model to subscribe to property change notifications</param>
    /// <param name="function">The function to compute the value of the dependent property</param>
    /// <returns>The configured dependent property</returns>
    IDependentNeedsChangesWith<T> Returns(IPropertyChangedHelper model, Func<T> function);

    /// <summary>
    /// Configures the dependent property with the specified model. The model is used to subscribe to property change notifications.
    /// The dependent property will be updated automatically when the specified properties of the model change.
    /// </summary>
    /// <param name="model">The model to subscribe to property change notifications</param>
    /// <returns>The configured dependent property</returns>
    IDependentNeedsReturns<T> WithModel(IPropertyChangedHelper model);
}

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// This dependent needs to be configured with its dependencies and update logic before it can be used.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
public interface IDependentNeedsReturns<T>
{
    /// <summary>
    /// Configures the dependent property with the specified update logic.
    /// </summary>
    /// <param name="function">The function to compute the value of the dependent property</param>
    /// <returns>The configured dependent property</returns>
    IDependentNeedsChangesWith<T> Returns(Func<T> function);
}

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// This dependent needs to be configured with its dependencies and update logic before it can be used.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
public interface IDependentNeedsChangesWith<T>
{
    /// <summary>
    /// Configures the dependent property to update its value when any of the specified properties change.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated
    /// </summary>
    /// <param name="propertyNames">The names of the properties that the dependent property depends on</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWith(params string[] propertyNames);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, 
    /// and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated
    /// </summary>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="getProperty">A function to get the property value</param>
    /// <param name="subPropertyNames">The names of the sub-properties to subscribe to</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithPropertyChanges(string propertyName, Func<INotifyPropertyChanged> getProperty, params string[] subPropertyNames);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, 
    /// and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated
    /// when the specified property changes. 
    /// Additionally, it will subscribe to the specified events on that property, allowing for more complex dependency scenarios.
    /// </summary>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="subscriber">A function to subscribe to the property's events</param>
    /// <param name="unsubscriber">A function to unsubscribe from the property's events</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithAndSubscribesTo(string propertyName, Action<EventHandler> subscriber, Action<EventHandler> unsubscriber);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, 
    /// and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated when the specified property changes.
    /// Additionally, it will subscribe to the specified events on that property, allowing for more complex dependency scenarios.
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="subscriber">A function to subscribe to the property's events</param>
    /// <param name="unsubscriber">A function to unsubscribe from the property's events</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithAndSubscribesTo<TEventArgs>(string propertyName, Action<EventHandler<TEventArgs>> subscriber, Action<EventHandler<TEventArgs>> unsubscriber);
}

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// This dependent is ready to be used and has been fully configured with its dependencies and update logic.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
public interface IDependentReady<T>
{
    /// <summary>
    /// Configures the dependent property to update its value when any of the specified properties change.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated when any of the specified properties change. 
    /// This allows for more complex dependency scenarios where the dependent property relies on multiple properties.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that the dependent property depends on</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWith(params string[] propertyNames);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, 
    /// and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated when the specified property changes.
    /// Additionally, it will subscribe to the specified events on that property, allowing for more complex dependency scenarios.
    /// </summary>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="getProperty">A delegate to get the property</param>
    /// <param name="subPropertyNames">The names of the sub-properties to subscribe to</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithPropertyChanges(string propertyName, Func<INotifyPropertyChanged> getProperty, params string[] subPropertyNames);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, 
    /// and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated when the specified property changes.
    /// Additionally, it will subscribe to the specified events on that property, allowing for more complex dependency scenarios.
    /// </summary>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="subscriber">A delegate to subscribe to the event</param>
    /// <param name="unsubscriber">A delegate to unsubscribe from the event</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithAndSubscribesTo(string propertyName, Action<EventHandler> subscriber, Action<EventHandler> unsubscriber);

    /// <summary>
    /// Configures the dependent property to update its value when the specified property changes, and subscribes to events on that property.
    /// The dependent property will subscribe to the PropertyChanged event of the model and will be updated when the specified property changes.
    /// Additionally, it will subscribe to the specified events on that property, allowing for more complex dependency scenarios.
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="propertyName">The name of the property that the dependent property depends on</param>
    /// <param name="subscriber">A delegate to subscribe to the event</param>
    /// <param name="unsubscriber">A delegate to unsubscribe from the event</param>
    /// <returns>The configured dependent property</returns>
    IDependentReady<T> ChangesWithAndSubscribesTo<TEventArgs>(string propertyName, Action<EventHandler<TEventArgs>> subscriber, Action<EventHandler<TEventArgs>> unsubscriber);
}

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
internal interface IDependent<T>
{
    /// <summary>
    /// Gets the value of the dependent property. The value is computed based on the configured dependencies and 
    /// is updated automatically when any of the dependent properties change.
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// The Dependent class allows for defining a property that relies on other properties and provides a mechanism
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
public class Dependent<T>
{
    /// <summary>
    /// Gets the value of the dependent property. 
    /// The value is computed based on the configured dependencies and is updated automatically when any of the dependent properties change.
    /// </summary>
    volatile private IDependent<T> _dependentProperty;

    /// <summary>
    /// Synchronizes access to the dependent property to ensure thread safety when initializing and updating the property value.
    /// </summary>
    private readonly Lock _Sync = new();

    /// <summary>
    /// Gets the value of the dependent property. If the property has not been initialized, it will be configured using the provided delegate.
    /// The delegate is responsible for defining the dependencies and the computation of the property's value.
    /// </summary>
    /// <param name="configureDependency">The delegate used to configure the dependencies.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the dependent property.</returns>
    public T Get(Func<IDependentNeedsModel<T>, IDependentReady<T>> configureDependency, [CallerMemberName] string propertyName = "")
    {
        if (_dependentProperty == null)
        {
            lock (_Sync)
            {
                if (_dependentProperty == null)
                {
                    // Configure dependent property behavior on first call.
                    var builder = new DependentBuilder<T>(propertyName);
                    configureDependency(builder);
                    _dependentProperty = builder.GetDependent();
                }
            }
        }

        return _dependentProperty.Value;
    }
}