using CommunityToolkit.Mvvm.ComponentModel;
using ModManager.StarCraft.Utilities.DependentProperties;

namespace ModManager.StarCraft.Application.ViewModels;

/// <summary>
/// Base class for view model implementation.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IPropertyChangingChangedHelper
{
    /// <summary>
    /// Links <c>OnPropertyChanged</c> for dependent property usage.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed</param>
    public void HelperPropertyChanged(string propertyName) => OnPropertyChanged(propertyName);

    /// <summary>
    /// Links <c>OnPropertyChanging</c> for dependent property usage.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed</param>
    public void HelperPropertyChanging(string propertyName) => OnPropertyChanging(propertyName);
}
