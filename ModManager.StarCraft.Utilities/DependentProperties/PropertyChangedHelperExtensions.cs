using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// Provides extension methods for IPropertyChangedHelper to facilitate property change notifications and dependent property management.
/// </summary>
public interface ISetResult
{
    /// <summary>
    /// Defines a method that allows chaining an action to be executed after a property has been set and the change has been notified.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The ISetResult instance.</returns>
    ISetResult Then(Action action);
}

/// <summary>
/// Defines an interface for a helper that notifies subscribers when a property has changed.
/// </summary>
public interface IPropertyChangedHelper : INotifyPropertyChanged
{
    /// <summary>
    /// Raises the PropertyChanged event for the specified property name, notifying subscribers that the property value has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    void HelperPropertyChanged(string propertyName);
}

/// <summary>
/// Defines an interface for a helper that notifies subscribers when a property is changing or has changed.
/// </summary>
public interface IPropertyChangingChangedHelper : INotifyPropertyChanging, IPropertyChangedHelper
{
    /// <summary>
    /// Raises the PropertyChanging event for the specified property name, notifying subscribers that the property value is about to change.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    void HelperPropertyChanging(string propertyName);
}

/// <summary>
/// Defines an interface for a helper that notifies subscribers when a property is unloading.
/// </summary>
public interface IPropertyUnloadingHelper
{
    /// <summary>
    /// Defines an event that is raised when a property is unloading, 
    /// allowing subscribers to perform actions before the property is unloaded.
    /// </summary>
    event Action PropertyUnloading;
}

/// <summary>
/// Provides extension methods for IPropertyChangedHelper to facilitate property change notifications and dependent property management.
/// </summary>
public static class PropertyChangedHelperExtensions
{
    /// <summary>
    /// A static instance of ISetResult that represents a no-change result.
    /// This instance is returned when a property set operation does not result in a change, 
    /// allowing for method chaining without executing any actions.
    /// </summary>
    private readonly static ISetResult NoChangeResult = new NoChangeSetResult();

    /// <summary>
    /// Sets the value of a property and raises the appropriate PropertyChanging and PropertyChanged events if the value has changed.
    /// This method checks if the new value is different from the current value, and if so, it raises the PropertyChanging event, 
    /// updates the field, and then raises the PropertyChanged event.
    /// It returns an ISetResult that allows for chaining an action to be executed after the property 
    /// has been set and the change has been notified.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="model">The model that contains the property.</param>
    /// <param name="field">The field that holds the property value.</param>
    /// <param name="newValue">The new value for the property.</param>
    /// <param name="caller">The name of the calling member.</param>
    /// <returns>The result of the set operation.</returns>
    public static ISetResult Set<T>(this IPropertyChangedHelper model, ref T field, T newValue, [CallerMemberName] string caller = "")
    {
        if (!Equals(field, newValue))
        {
            (model as IPropertyChangingChangedHelper)?.HelperPropertyChanging(caller);
            field = newValue;
            model.HelperPropertyChanged(caller);
            return new PropertyChangedSetResult();
        }

        return NoChangeResult;
    }

    /// <summary>
    /// Gets the value of a dependent property, initializing it if necessary, and configuring its dependencies using the provided function.
    /// This method checks if the dependent property field is null, and if so, it initializes it. 
    /// It then retrieves the value of the dependent property, configuring its dependencies using the provided function.
    /// </summary>
    /// <typeparam name="T">The type of the dependent property.</typeparam>
    /// <param name="model">The model that contains the dependent property.</param>
    /// <param name="field">The field that holds the dependent property value.</param>
    /// <param name="configureChanges">A function to configure the dependencies of the dependent property.</param>
    /// <param name="caller">The name of the calling member.</param>
    /// <returns>The value of the dependent property.</returns>
    public static T Get<T>(this IPropertyChangedHelper model, ref Dependent<T> field, Func<IDependentNeedsReturns<T>, IDependentReady<T>> configureChanges, [CallerMemberName] string caller = "")
    {
        if (field == null)
        {
            var newField = new Dependent<T>();
            Interlocked.CompareExchange(ref field, newField, null);
        }

        return field.Get(x => configureChanges(x.WithModel(model)), caller);
    }

    /// <summary>
    /// Gets the value of a dependent property, initializing it if necessary, and configuring its dependencies using the provided function.
    /// This method checks if the dependent property field is null, and if so, it initializes it. 
    /// It then retrieves the value of the dependent property, configuring its dependencies using the provided function.
    /// </summary>
    /// <typeparam name="TModel">The type of the model that contains the dependent property.</typeparam>
    /// <typeparam name="TProp">The type of the dependent property.</typeparam>
    /// <param name="model">The model that contains the dependent property.</param>
    /// <param name="field">The field that holds the dependent property value.</param>
    /// <param name="expressionBuilder">A function to build the expression for the dependent property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the dependent property.</returns>
    public static TProp Get<TModel, TProp>(
        this TModel model,
        ref Dependent<TProp> field,
        Func<Expression<Func<TModel, TProp>>> expressionBuilder,
        [CallerMemberName] string propertyName = "")
        where TModel : IPropertyChangedHelper
    {
        if (field == null)
        {
            var newField = new Dependent<TProp>();
            Interlocked.CompareExchange(ref field, newField, null);
        }

        return field.Get(x =>
        {
            var dependentBuilder = ExpressionDependentBuilderCache<TModel, TProp>.Get(propertyName, expressionBuilder);
            return dependentBuilder.Build(model, (DependentBuilder<TProp>)x);
        }, propertyName);
    }

    /// <summary>
    /// A private class that implements ISetResult and represents a no-change result.
    /// This class is used to indicate that a property set operation did not result in a change, 
    /// allowing for method chaining without executing any actions.
    /// It is used internally by the Set extension method to provide a consistent return type for cases where no change occurs.
    /// </summary>
    private class NoChangeSetResult : ISetResult
    {
        /// <summary>
        /// Executes the specified action and returns the current instance of ISetResult.
        /// This method is used to allow for method chaining after a property set operation,
        /// even when no change has occurred. The action is executed immediately, 
        /// and the current instance of ISetResult is returned to allow for further chaining of actions.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>The current instance of ISetResult.</returns>
        public ISetResult Then(Action action) => this;
    }

    /// <summary>
    /// A private struct that implements ISetResult and represents the result of a property set operation that resulted in a change.
    /// This struct is used to indicate that a property set operation resulted in a change, 
    /// allowing for method chaining to execute additional actions after the change has been notified.
    /// It is used internally by the Set extension method to provide a consistent return type for cases where a change occurs, 
    /// enabling further actions to be executed after the property has been set and the change has been notified.
    /// </summary>
    private struct PropertyChangedSetResult : ISetResult
    {
        /// <summary>
        /// Executes the specified action and returns the current instance of ISetResult.
        /// This method is used to allow for method chaining after a property set operation that resulted in a change.
        /// The action is executed immediately, 
        /// and the current instance of ISetResult is returned to allow for further chaining of actions. 
        /// This enables additional logic to be performed after the property
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>The current instance of ISetResult.</returns>
        public readonly ISetResult Then(Action action)
        {
            action();
            return this;
        }
    }
}