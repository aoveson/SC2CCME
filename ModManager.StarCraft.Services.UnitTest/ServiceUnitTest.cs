using System.Collections;
using System.Reflection;
using ModManager.StarCraft.Services;

namespace ModManager.StarCraft.Services.UnitTest;

/// <summary>Verifies singleton registration and retrieval behavior.</summary>
public sealed class ServiceUnitTest
{
    /// <summary>Returns the same registered instance on repeated retrieval.</summary>
    [Fact]
    public void CreateAndGetReturnsTheRegisteredSingleton()
    {
        Service.Remove<TestService>();

        Service.Create<TestService>();

        Assert.Same(Service.Get<TestService>(), Service.Get<TestService>());
        Service.Remove<TestService>();
    }

    /// <summary>Rejects duplicate registration through <see cref="Service.Create{T}"/>.</summary>

    /// <summary>Replaces registered instances and removes them safely.</summary>

    /// <summary>Rejects a registry entry whose runtime type does not match the requested service.</summary>
    [Fact]
    public void CreateThrowsWhenTheServiceAlreadyExists()
    {
        Service.SetInstance(new TestService());

        Assert.Throws<InvalidOperationException>(() => Service.Create<TestService>());
        Service.Remove<TestService>();
    }

    [Fact]
    public void SetInstanceReplacesAndRemoveDeletesTheService()
    {
        TestService first = new();
        TestService second = new();
        Service.SetInstance(first);

        Service.SetInstance(second);

        Assert.Same(second, Service.Get<TestService>());
        Service.Remove<TestService>();
        Service.Remove<TestService>();
        Assert.Throws<KeyNotFoundException>(() => Service.Get<TestService>());
    }

    [Fact]
    public void GetThrowsWhenTheRegisteredValueHasTheWrongType()
    {
        FieldInfo field = typeof(Service).GetField("_services", BindingFlags.Static | BindingFlags.NonPublic)!;
        IDictionary services = (IDictionary)field.GetValue(null)!;
        services[typeof(TestService)] = "wrong type";

        try
        {
            Assert.Throws<InvalidDataException>(() => Service.Get<TestService>());
        }
        finally
        {
            Service.Remove<TestService>();
        }
    }

    private sealed class TestService
    {
    }
}
