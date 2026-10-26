using ModManager.StarCraft.Application.ViewModels;

namespace ModManager.StarCraft.Application.UnitTest;

/// <summary>
/// Verifies the notification bridge exposed by <see cref="ViewModelBase"/>.
/// </summary>
public sealed class ViewModelBaseUnitTest
{
    /// <summary>
    /// Ensures the helper methods raise the corresponding property notifications.
    /// </summary>
    [Fact]
    public void HelperNotificationMethodsRaisePropertyNotifications()
    {
        TestViewModel viewModel = new();
        List<string> changing = [];
        List<string> changed = [];
        viewModel.PropertyChanging += (_, args) => changing.Add(args.PropertyName!);
        viewModel.PropertyChanged += (_, args) => changed.Add(args.PropertyName!);

        viewModel.HelperPropertyChanging(nameof(TestViewModel.Value));
        viewModel.HelperPropertyChanged(nameof(TestViewModel.Value));

        Assert.Equal([nameof(TestViewModel.Value)], changing);
        Assert.Equal([nameof(TestViewModel.Value)], changed);
    }

    private sealed class TestViewModel : ViewModelBase
    {
        public int Value { get; set; }
    }
}