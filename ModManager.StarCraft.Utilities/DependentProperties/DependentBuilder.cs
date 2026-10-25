using System;
using System.ComponentModel;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// Represents a property that is dependent on other properties and automatically updates its value when those properties change.
/// This dependent needs to be configured with its dependencies and update logic before it can be used.
/// </summary>
/// <typeparam name="T">The type of the dependent property</typeparam>
/// <remarks>
/// Initializes a new instance of the DependentBuilder class with the specified caller name. 
/// The caller name is used for logging and debugging purposes to identify which property is being created.
/// </remarks>
/// <param name="caller"></param>
internal class DependentBuilder<T>(string caller) :
    IDependentNeedsModel<T>,
    IDependentNeedsReturns<T>,
    IDependentNeedsChangesWith<T>,
    IDependentReady<T>
{
    /// <summary>
    /// The name of the caller that is creating this dependent property. 
    /// This is used for logging and debugging purposes to identify which property is being created.
    /// </summary>
    private readonly string _caller = caller;

    /// <summary>
    /// The model that implements IPropertyChangedHelper, which is used to subscribe to property change notifications. 
    /// This model is required to determine when the dependent property needs to be updated.
    /// </summary>
    private IPropertyChangedHelper _model;

    /// <summary>
    /// The dependent property that is being built. 
    /// This property will be configured with the specified update logic and dependencies, 
    /// and will automatically update its value when any of the dependent properties change.
    /// </summary>
    private DependentOnPropertyChanged<T> _dependent;

    /// <summary>
    /// Configures the dependent property with the specified model and update logic. 
    /// The model is used to subscribe to property change notifications, 
    /// and the update logic is used to compute the value of the dependent property.
    /// </summary>
    /// <param name="model">The model that implements IPropertyChangedHelper</param>
    /// <param name="function">The function used to compute the value of the dependent property</param>
    /// <returns></returns>
    public IDependentNeedsChangesWith<T> Returns(IPropertyChangedHelper model, Func<T> function)
        => WithModel(model).Returns(function);

    /// <summary>
    /// Configures the dependent property with the specified model. 
    /// The model is used to subscribe to property change notifications, 
    /// which will trigger updates to the dependent property when any of the dependent properties change.
    /// </summary>
    /// <param name="model">The model that implements IPropertyChangedHelper</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentNeedsReturns<T> WithModel(IPropertyChangedHelper model) {
        _model = model;
        return this;
    }

    /// <summary>
    /// Configures the dependent property with the specified update logic. 
    /// The update logic is used to compute the value of the dependent property based on the values of the dependent properties.
    /// </summary>
    /// <param name="function">The function used to compute the value of the dependent property</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentNeedsChangesWith<T> Returns(Func<T> function) {
        if (_model is IPropertyChangingChangedHelper modelChangingChanged)
        {
            _dependent = new DependentOnPropertyChangingChanged<T>(
                function,
                () => {
                    modelChangingChanged.HelperPropertyChanged(_caller);
                },
                () => {
                    modelChangingChanged.HelperPropertyChanging(_caller);
                });
        }
        else
        {
            _dependent = new DependentOnPropertyChanged<T>(
                function,
                () => {
                    _model.HelperPropertyChanged(_caller);
                });
        }

        return this;
    }

    /// <summary>
    /// Configures the dependent property to change when any of the specified properties change.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that trigger updates to the dependent property</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentReady<T> ChangesWith(params string[] propertyNames)
    {            
        _model.PropertyChanged += _dependent.RefreshFor(propertyNames);
        return this;
    }

    /// <summary>
    /// Configures the dependent property to change when the specified property changes, and also subscribes to changes in its sub-properties.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="getProperty">A function that returns the sub-property's model</param>
    /// <param name="subPropertyNames">The names of the sub-properties that trigger updates to the dependent property</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentReady<T> ChangesWithPropertyChanges(string propertyName, Func<INotifyPropertyChanged> getProperty, params string[] subPropertyNames)
    {
        ChangesWith(propertyName);

        var source = (IPropertyChangingChangedHelper)_model;

        PropertyChangedEventHandler subRefresh = _dependent.RefreshFor(subPropertyNames);
        PropertyChangingEventHandler unhook = Unhook(propertyName, getProperty, subRefresh);
        PropertyChangedEventHandler hook = Hook(propertyName, getProperty, subRefresh);

        source.PropertyChanging += unhook;
        source.PropertyChanged += hook;

        // Ensure subscribed to current value.
        hook.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Unsubscribe to the property
        if (source is IPropertyUnloadingHelper unsubscribeHelper)
        {
            unsubscribeHelper.PropertyUnloading += () =>
            {
                unhook.Invoke(this, new PropertyChangingEventArgs(propertyName));
            };
        }
        return this;
    }

    /// <summary>
    /// Configures the dependent property to change when the specified property changes, 
    /// and subscribes to changes in its sub-properties.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="subscriber">The action to subscribe to property changes</param>
    /// <param name="unsubscriber">The action to unsubscribe from property changes</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentReady<T> ChangesWithAndSubscribesTo(string propertyName, Action<EventHandler> subscriber, Action<EventHandler> unsubscriber)
    {
        ChangesWith(propertyName);

        var source = (IPropertyChangingChangedHelper)_model;
        source.PropertyChanging += Unhook(propertyName, unsubscriber);
        source.PropertyChanged += Hook(propertyName, subscriber);
        subscriber(_dependent.Refresh);

        // Unsubscribe to the property
        if (source is IPropertyUnloadingHelper unsubscribeHelper)
        { 
            unsubscribeHelper.PropertyUnloading += () =>
            {
                unsubscriber?.Invoke(_dependent.Refresh);
            };
        }
        return this;
    }

    /// <summary>
    /// Configures the dependent property to change when the specified property changes, 
    /// and subscribes to changes in its sub-properties.
    /// </summary>
    /// <typeparam name="TArgs">The type of the event arguments</typeparam>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="subscriber">The action to subscribe to property changes</param>
    /// <param name="unsubscriber">The action to unsubscribe from property changes</param>
    /// <returns>A configuration object for the dependent property</returns>
    public IDependentReady<T> ChangesWithAndSubscribesTo<TArgs>(string propertyName, Action<EventHandler<TArgs>> subscriber, Action<EventHandler<TArgs>> unsubscriber)
    {
        ChangesWith(propertyName);

        var source = (IPropertyChangingChangedHelper)_model;
        source.PropertyChanging += Unhook(propertyName, unsubscriber);
        source.PropertyChanged += Hook(propertyName, subscriber);
        subscriber(_dependent.Refresh);

        // Unsubscribe to the property
        if (source is IPropertyUnloadingHelper unsubscribeHelper)
        {
            unsubscribeHelper.PropertyUnloading += () =>
            {
                unsubscriber?.Invoke(_dependent.Refresh);
            };
        }
        return this;
    }

    /// <summary>
    /// Unhooks the specified property change event handler from the model when the specified property changes. 
    /// This is used to unsubscribe from property change notifications when the dependent property is no longer interested
    /// in updates from that property.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="unhook">The action to unsubscribe from property changes</param>
    /// <returns>The event handler for unhooking property changes</returns>
    private PropertyChangingEventHandler Unhook(string propertyName, Action<EventHandler> unhook)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                unhook(_dependent.Refresh);
            }
        };
    }

    /// <summary>
    /// Hooks the specified property change event handler to the model when the specified property changes. 
    /// This is used to subscribe to property change notifications when the dependent property is interested in updates from that property.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="hook">The action to subscribe to property changes</param>
    /// <returns>The event handler for hooking property changes</returns>
    private PropertyChangedEventHandler Hook(string propertyName, Action<EventHandler> hook)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                hook(_dependent.Refresh);
            }
        };
    }

    /// <summary>
    /// Unhooks the specified property change event handler from the model when the specified property changes. 
    /// This is used to unsubscribe from property change notifications when the dependent property is no longer interested 
    /// in updates from that property.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="getModel">A function to get the model instance</param>
    /// <param name="subRefresh">The event handler to unsubscribe</param>
    /// <returns>The event handler for unhooking property changes</returns>
    private PropertyChangingEventHandler Unhook(string propertyName, Func<INotifyPropertyChanged> getModel, PropertyChangedEventHandler subRefresh)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                var model = getModel();
                model?.PropertyChanged -= subRefresh;
            }
        };
    }

    /// <summary>
    /// Hooks the specified property change event handler to the model when the specified property changes. 
    /// This is used to subscribe to property change notifications when the dependent property 
    /// is interested in updates from that property.
    /// </summary>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="getModel">A function to get the model instance</param>
    /// <param name="subRefresh">The event handler to subscribe</param>
    /// <returns>The event handler for hooking property changes</returns>
    private PropertyChangedEventHandler Hook(string propertyName, Func<INotifyPropertyChanged> getModel, PropertyChangedEventHandler subRefresh)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                var model = getModel();
                if (model != null)
                    model.PropertyChanged += subRefresh;
            }
        };
    }

    /// <summary>
    /// Unhooks the specified property change event handler from the model when the specified property changes. 
    /// This is used to unsubscribe from property change notifications when the dependent property 
    /// is no longer interested in updates from that property.
    /// </summary>
    /// <typeparam name="Targs">The type of the event arguments</typeparam>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="unhook">The event handler to unsubscribe</param>
    /// <returns>The event handler for unhooking property changes</returns>
    private PropertyChangingEventHandler Unhook<Targs>(string propertyName, Action<EventHandler<Targs>> unhook)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                unhook(_dependent.Refresh);
            }
        };
    }

    /// <summary>
    /// Hooks the specified property change event handler to the model when the specified property changes. 
    /// This is used to subscribe to property change notifications when the dependent property is interested in updates from that property.
    /// </summary>
    /// <typeparam name="Targs">The type of the event arguments</typeparam>
    /// <param name="propertyName">The name of the property that triggers updates to the dependent property</param>
    /// <param name="hook">The event handler to subscribe</param>
    /// <returns>The event handler for hooking property changes</returns>
    private PropertyChangedEventHandler Hook<Targs>(string propertyName, Action<EventHandler<Targs>> hook)
    {
        return (s, e) =>
        {
            if (propertyName == e.PropertyName)
            {
                hook(_dependent.Refresh);
            }
        };
    }

    /// <summary>
    /// Gets the dependent property that has been configured with its dependencies and update logic. 
    /// The value of this property is automatically updated when any of the dependent properties change.
    /// </summary>
    /// <returns>The dependent property</returns>
    internal IDependent<T> GetDependent() => _dependent;
}
