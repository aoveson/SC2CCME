using System.ComponentModel;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// A base class that implements the IPropertyChangingChangedHelper interface, 
/// providing a mechanism for notifying subscribers when a property is changing or has changed.
/// </summary>
public class PropertyChangedHelperBase : IPropertyChangingChangedHelper
{
    /// <summary>
    /// An event that is raised when a property is changing. 
    /// Subscribers can use this event to perform actions before the property value is updated.
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// An event that is raised when a property has changed.
    /// Subscribers can use this event to perform actions after the property value has been updated.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property name, notifying subscribers that the property value has changed.
    /// </summary>
    /// <param name="propertyName"></param>
    public virtual void HelperPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    /// Raises the PropertyChanging event for the specified property name, notifying subscribers that the property value is about to change.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    public virtual void HelperPropertyChanging(string propertyName) 
        => PropertyChanging?.Invoke(this, new(propertyName));
}