using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// A cache for ExpressionDependentBuilder instances, keyed by property name. 
/// This allows for reusing the same ExpressionDependentBuilder for the same property name across different instances of TModel.
/// </summary>
/// <typeparam name="TModel">The type of the model</typeparam>
/// <typeparam name="T">The type of the dependent property</typeparam>
internal static class ExpressionDependentBuilderCache<TModel, T> where TModel : IPropertyChangedHelper
{
    /// <summary>
    /// A thread-safe dictionary that caches ExpressionDependentBuilder instances, keyed by property name. 
    /// This allows for reusing the same ExpressionDependentBuilder for the same property name across different instances of TModel. 
    /// The cache is used to avoid the overhead of creating new ExpressionDependentBuilder instances
    /// </summary>
    private static readonly ConcurrentDictionary<string, ExpressionDependentBuilder<TModel, T>> _Cache = new();

    /// <summary>
    /// Gets an ExpressionDependentBuilder for the specified property name and expression builder.
    /// If an ExpressionDependentBuilder for the property name already exists in the cache, it is
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="expressionBuilder">The expression builder</param>
    /// <returns>The ExpressionDependentBuilder</returns>
    public static ExpressionDependentBuilder<TModel, T> Get(string propertyName, Func<Expression<Func<TModel, T>>> expressionBuilder)
    {
        return _Cache.GetOrAdd(propertyName, _ => new ExpressionDependentBuilder<TModel, T>(expressionBuilder));
    }
}
