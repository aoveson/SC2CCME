namespace ModManager.StarCraft.Services;

/// <summary>
/// Handles the creation and management of singleton services.
/// </summary>
public static class Service
{
    private static readonly Dictionary<Type, object> _services = [];

    /// <summary>
    /// Creates a new singleton instance using the default constructor.
    /// </summary>
    /// <typeparam name="T">The service type to create.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the service is already registered in the singleton dictionary.
    /// </exception>
    /// <remarks>
    /// Requires an empty constructor to exist for the type. Creating an empty constructor and proper setters is recommended.
    /// </remarks>
    public static void Create<T>() where T : new()
    {
        T newService = new();
        if(!_services.ContainsKey(typeof(T)))
        {
            _services.Add(typeof(T), newService);
        }
        else
        {
            throw new InvalidOperationException($"Service of type: {typeof(T)} already registered!");
        }
    }

    /// <summary>
    /// Sets a service instance to a defined value, overwriting the old instance if applicable.
    /// </summary>
    /// <typeparam name="T">The type of service being modified.</typeparam>
    /// <param name="newInstance">The new instance to insert.</param>
    public static void SetInstance<T>(T newInstance)
    {
        if(_services.ContainsKey(typeof(T)))
        {
            _services.Remove(typeof(T));
        }

        _services.Add(typeof(T), newInstance);
    }

    /// <summary>
    /// Removes the singleton instance of the service from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type to remove.</typeparam>
    public static void Remove<T>()
    {
        if(_services.ContainsKey(typeof(T)))
        {
            _services.Remove(typeof(T));
        }
    }

    /// <summary>
    /// Gets the singleton instance of a service based on the type provided.
    /// </summary>
    /// <typeparam name="T">The type of service singleton to get.</typeparam>
    /// <returns>The singleton instance.</returns>
    /// <exception cref="InvalidDataException">Thrown if the returned instance is not of the requested type.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the requested singleton instance does not exist in the dictionary.</exception>
    public static T Get<T>()
    {
        if(_services.TryGetValue(typeof(T), out object outVal))
        {
            if(outVal is T typed)
            {
                return typed;
            }
            else
            {
                throw new InvalidDataException($"Service was not the correct type!");
            }
        }
        else
        {
            throw new KeyNotFoundException($"Could not find service of type: {typeof(T)}! Did you previously call {nameof(Create)}?");
        }
    }
}