using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ModManager.StarCraft.Utilities.DependentProperties;

namespace ModManager.StarCraft.Utilities.UnitTest;

/// <summary>
/// Verifies dependent-property invalidation, collection reflection, and notification subscriptions.
/// </summary>
public sealed class DependentPropertiesUnitTest
{
    /// <summary>Raises the expected collection events while adding and truncating ranges.</summary>
    [Fact]
    public void FastObservableCollectionAddsAndTruncatesRanges()
    {
        FastObservableCollection<int> collection = new([1]);
        List<NotifyCollectionChangedAction> changes = [];
        collection.CollectionChanged += (_, args) => changes.Add(args.Action);

        collection.AddRange([]);
        collection.AddRange([2]);
        collection.AddRange([3, 4]);
        collection.Truncate(3);
        collection.Truncate(1);
        collection.Truncate(0);

        Assert.Empty(collection);
        Assert.Equal(
            [NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Reset,
             NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Reset,
             NotifyCollectionChangedAction.Remove],
            changes);
    }

    /// <summary>Rejects null ranges and negative truncation sizes.</summary>
    [Fact]
    public void FastObservableCollectionRejectsInvalidRanges()
    {
        FastObservableCollection<int> collection = new();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => collection.Truncate(-1));
    }

    /// <summary>Updates existing reflected items, adds new items, and removes extras.</summary>
    [Fact]
    public void ReflectUpdatesExistingItemsAddsNewItemsAndRemovesExtras()
    {
        FastObservableCollection<Item> target = new([new Item(), new Item(), new Item()]);
        List<int> source = [1, 2];

        target.Reflect(source, (item, value) => item.Value = value);

        Assert.Equal([1, 2], target.Select(item => item.Value));

        target.Reflect([3, 4, 5, 6], (item, value) => item.Value = value);

        Assert.Equal([3, 4, 5, 6], target.Select(item => item.Value));

        target.Reflect<Item, int>(null!, (item, value) => item.Value = value);

        Assert.Empty(target);
    }

    /// <summary>Raises property notifications only when a property value changes.</summary>
    [Fact]
    public void PropertyChangedSetRaisesEventsOnlyWhenValueChanges()
    {
        TestModel model = new();
        List<string> changing = [];
        List<string> changed = [];
        model.PropertyChanging += (_, args) => changing.Add(args.PropertyName!);
        model.PropertyChanged += (_, args) => changed.Add(args.PropertyName!);

        model.Name = "first";
        model.Name = "first";

        Assert.Equal(["Name"], changing);
        Assert.Equal(["Name"], changed);
    }

    /// <summary>Runs chained actions only when a value was changed.</summary>
    [Fact]
    public void PropertySetResultsRunActionsOnlyForChanges()
    {
        TestModel model = new();
        int field = 1;
        int actions = 0;

        model.Set(ref field, 2).Then(() => actions++);
        model.Set(ref field, 2).Then(() => actions++);

        Assert.Equal(2, field);
        Assert.Equal(1, actions);
    }

    /// <summary>Refreshes a dependent value after its declared property changes.</summary>
    [Fact]
    public void DependentPropertyRefreshesForChangedProperties()
    {
        TestModel model = new() { Name = "one" };
        Dependent<int> dependent = new();
        int notifications = 0;
        model.PropertyChanged += (_, _) => notifications++;

        int value = model.Get(ref dependent, configure =>
            configure.Returns(() => model.Name.Length).ChangesWith(nameof(TestModel.Name)));

        model.Name = "longer";

        Assert.Equal(3, value);
        Assert.Equal(6, model.Get(ref dependent, configure =>
            configure.Returns(() => model.Name.Length).ChangesWith(nameof(TestModel.Name))));
        Assert.True(notifications > 0);
    }

    /// <summary>Tracks direct, nested, computed, constant, identity, and duplicate expression dependencies.</summary>
    [Fact]
    public void ExpressionDependentTracksDirectAndNestedProperties()
    {
        TestModel model = new() { Name = "one", Child = new ChildModel { Value = 2 } };
        Dependent<int> direct = null!;
        Dependent<int> nested = null!;
        Dependent<int> computed = null!;
        Dependent<int> constant = null!;
        Dependent<TestModel> identity = null!;
        Dependent<int> duplicate = null!;

        Assert.Equal(3, model.Get(ref direct, () => value => value.Name.Length, "Direct"));
        Assert.Equal(2, model.Get(ref nested, () => value => value.Child.Value, "Nested"));
        Assert.Equal(4, model.Get(ref computed, () => value => value.Name.Length + 1, "Computed"));
        Assert.Equal(42, model.Get(ref constant, () => value => 42, "Constant"));
        Assert.Same(model, model.Get(ref identity, () => value => value, "Identity"));
        Assert.Equal(6, model.Get(ref duplicate, () => value => value.Name.Length + value.Name.Length, "Duplicate"));

        model.Name = "longer";
        model.Child.Value = 5;

        Assert.Equal(6, model.Get(ref direct, () => value => value.Name.Length, "Direct"));
        Assert.Equal(5, model.Get(ref nested, () => value => value.Child.Value, "Nested"));
        Assert.Equal(7, model.Get(ref computed, () => value => value.Name.Length + 1, "Computed"));
    }

    /// <summary>Runs both changing and changed callbacks for a dependent value.</summary>
    [Fact]
    public void ChangingChangedDependentRunsChangingAndChangedCallbacks()
    {
        TestModel model = new() { Name = "one" };
        Dependent<int> dependent = new();
        List<string> events = [];

        int value = dependent.Get(configure =>
            configure.Returns(model, () => model.Name.Length)
                .ChangesWith(nameof(TestModel.Name)));
        model.PropertyChanging += (_, _) => events.Add("model-changing");
        model.PropertyChanged += (_, _) => events.Add("model-changed");
        model.Name = "longer";

        Assert.Equal(3, value);
        Assert.Equal(6, dependent.Get(configure =>
            configure.Returns(model, () => model.Name.Length)
                .ChangesWith(nameof(TestModel.Name))));
        Assert.Contains("model-changing", events);
        Assert.Contains("model-changed", events);
    }

    /// <summary>Tracks nested property changes and replacement of the nested object.</summary>
    [Fact]
    public void DependentTracksNestedPropertyReplacement()
    {
        TestModel model = new() { Child = new ChildModel { Value = 2 } };
        Dependent<int> dependent = new();

        Assert.Equal(2, dependent.Get(configure => configure
            .Returns(model, () => model.Child.Value)
            .ChangesWithPropertyChanges(nameof(TestModel.Child), () => model.Child, nameof(ChildModel.Value))));

        model.Child.Value = 3;
        Assert.Equal(3, dependent.Get(configure => configure
            .Returns(model, () => model.Child.Value)
            .ChangesWithPropertyChanges(nameof(TestModel.Child), () => model.Child, nameof(ChildModel.Value))));

        model.Child = new ChildModel { Value = 4 };
        Assert.Equal(4, dependent.Get(configure => configure
            .Returns(model, () => model.Child.Value)
            .ChangesWithPropertyChanges(nameof(TestModel.Child), () => model.Child, nameof(ChildModel.Value))));

        model.Unload();
    }

    /// <summary>Supports a notifying model that does not implement property-changing notifications.</summary>
    [Fact]
    public void DependentSupportsModelsWithoutPropertyChanging()
    {
        BasicModel model = new();
        Dependent<int> dependent = new();
        int field = 1;

        model.Set(ref field, 2);

        Assert.Equal(1, dependent.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWith(nameof(BasicModel.Value))));

        model.Value = 2;

        Assert.Equal(2, dependent.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWith(nameof(BasicModel.Value))));
    }

    /// <summary>Updates dependent values from regular and generic event subscriptions.</summary>
    [Fact]
    public void DependentSupportsEventSubscriptions()
    {
        EventModel model = new();
        Dependent<int> regular = new();
        Dependent<int> generic = new();

        Assert.Equal(0, regular.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWithAndSubscribesTo(nameof(EventModel.Signal),
                handler => model.Signal += handler,
                handler => model.Signal -= handler)));
        Assert.Equal(0, generic.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWithAndSubscribesTo<EventArgs>(nameof(EventModel.GenericSignal),
                handler => model.GenericSignal += handler,
                handler => model.GenericSignal -= handler)));

        model.Value = 5;
        model.RaiseSignals();

        Assert.Equal(5, regular.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWithAndSubscribesTo(nameof(EventModel.Signal),
                handler => model.Signal += handler,
                handler => model.Signal -= handler)));
        Assert.Equal(5, generic.Get(configure => configure
            .Returns(model, () => model.Value)
            .ChangesWithAndSubscribesTo<EventArgs>(nameof(EventModel.GenericSignal),
                handler => model.GenericSignal += handler,
                handler => model.GenericSignal -= handler)));

        model.HelperPropertyChanging("Other");
        model.HelperPropertyChanged("Other");
        model.HelperPropertyChanging(nameof(EventModel.Signal));
        model.HelperPropertyChanged(nameof(EventModel.Signal));
        model.HelperPropertyChanging(nameof(EventModel.GenericSignal));
        model.HelperPropertyChanged(nameof(EventModel.GenericSignal));
        model.Unload();
        model.RaiseSignals();
    }

    /// <summary>Handles non-notifying collections and rejects unsupported long dependency chains.</summary>
    [Fact]
    public void ExpressionDependenciesHandleNonNotifyingAndLongChains()
    {
        TestModel model = new() { Items = [1, 2], Child = new ChildModel { GrandChild = new GrandChildModel { Value = 3 } } };
        Dependent<int> count = null!;
        Dependent<int> longChain = null!;

        Assert.Equal(2, model.Get(ref count, () => value => value.Items.Count, "ItemCount"));
        Assert.Throws<NotImplementedException>(() => model.Get(ref longChain,
            () => value => value.Child.GrandChild.Value, "LongChain"));
    }

    /// <summary>Reflects source collection changes into a stable observable collection instance.</summary>
    [Fact]
    public void ReturnsCollectionReflectsSourceChanges()
    {
        TestModel model = new() { Items = [1, 2] };
        Dependent<ObservableCollection<Item>> dependent = null!;

        ObservableCollection<Item> first = model.Get(ref dependent, configure =>
            configure.ReturnsCollection(() => model.Items, (item, value) => item.Value = value)
                .ChangesWith(nameof(TestModel.Items)));
        model.Items = [3, 4, 5];
        ObservableCollection<Item> second = model.Get(ref dependent, configure =>
            configure.ReturnsCollection(() => model.Items, (item, value) => item.Value = value)
                .ChangesWith(nameof(TestModel.Items)));

        Assert.Same(first, second);
        Assert.Equal([3, 4, 5], second.Select(item => item.Value));
    }

    private sealed class Item
    {
        public int Value { get; set; }
    }

    private sealed class ChildModel : PropertyChangedHelperBase
    {
        private int _value;
        private GrandChildModel _grandChild = new();

        public int Value
        {
            get => _value;
            set => this.Set(ref _value, value);
        }

        public GrandChildModel GrandChild
        {
            get => _grandChild;
            set => this.Set(ref _grandChild, value);
        }
    }

    private sealed class GrandChildModel : PropertyChangedHelperBase
    {
        private int _value;

        public int Value
        {
            get => _value;
            set => this.Set(ref _value, value);
        }
    }

    private sealed class BasicModel : IPropertyChangedHelper
    {
        private int _value = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new(nameof(Value)));
            }
        }

        public void HelperPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new(propertyName));
    }

    private sealed class EventModel : PropertyChangedHelperBase, IPropertyUnloadingHelper
    {
        private int _value;

        public event EventHandler? Signal;
        public event EventHandler<EventArgs>? GenericSignal;
        public event Action? PropertyUnloading;

        public int Value
        {
            get => _value;
            set => this.Set(ref _value, value);
        }

        public void RaiseSignals()
        {
            Signal?.Invoke(this, EventArgs.Empty);
            GenericSignal?.Invoke(this, EventArgs.Empty);
        }

        public void Unload() => PropertyUnloading?.Invoke();
    }

    private sealed class TestModel : PropertyChangedHelperBase, IPropertyUnloadingHelper
    {
        private string _name = string.Empty;
        private ChildModel _child = new();
        private List<int> _items = [];

        public event Action? PropertyUnloading;

        public string Name
        {
            get => _name;
            set => this.Set(ref _name, value);
        }

        public ChildModel Child
        {
            get => _child;
            set => this.Set(ref _child, value);
        }

        public List<int> Items
        {
            get => _items;
            set => this.Set(ref _items, value);
        }

        public void Unload() => PropertyUnloading?.Invoke();
    }
}
