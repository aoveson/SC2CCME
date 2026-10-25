using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ModManager.StarCraft.Utilities.DependentProperties;

/// <summary>
/// A builder class that constructs dependent properties based on expressions. 
/// It analyzes the expression to determine the dependencies and sets up the necessary change notifications.
/// </summary>
/// <typeparam name="TModel">The type of the model that contains the properties</typeparam>
/// <typeparam name="T">The type of the dependent property</typeparam>
internal class ExpressionDependentBuilder<TModel, T> where TModel : IPropertyChangedHelper
{
    /// <summary>
    /// A list of steps that define how to build the dependent property based on the provided expression.
    /// Each step is an action that takes the model and the dependent builder, and configures the dependent property accordingly.
    /// </summary>
    private readonly List<Action<TModel, DependentBuilder<T>>> _steps = [];

    /// <summary>
    /// Initializes a new instance of the ExpressionDependentBuilder class with the specified expression builder.
    /// The expression builder is a function that returns an expression representing the dependent property.
    /// The constructor analyzes the expression to determine the dependencies and sets up the necessary change notifications.
    /// </summary>
    /// <param name="expressionBuilder">The function that returns an expression representing the dependent property</param>
    /// <exception cref="NotImplementedException">Thrown when the expression contains unsupported constructs</exception>
    public ExpressionDependentBuilder(Func<Expression<Func<TModel, T>>> expressionBuilder)
    {
        var expression = expressionBuilder();
        var compiledExpressionCaller = expression.Compile();
        _steps.Add((model, builder) => builder.Returns(model, () => compiledExpressionCaller(model)));

        var paths = GetChangesWithPaths(expression);

        foreach (var path in paths)
        {
            // Path in the format "x.Child[.GrandChild]" where x is this.
            var props = path.Split('.');
            if (props.Length == 2)
            {
                _steps.Add((model, builder) => builder.ChangesWith(props[1]));
            }
            else if (props.Length == 3)
            {
                var param = Expression.Parameter(typeof(TModel), props[0]);
                var partial = Expression.Lambda<Func<TModel, IPropertyChangingChangedHelper>>(
                        Expression.Property(param, props[1]),
                        new[] { param });
                var getIntermediateProperty = partial.Compile();

                _steps.Add((model, builder) => builder.ChangesWithPropertyChanges(
                    props[1],
                    () => getIntermediateProperty(model),
                    props[2]));
            }
            else if (props.Length > 3)
            {
                throw new NotImplementedException("Cannot support long property chain: " + path);
            }
        }
    }

    public IDependentReady<T> Build(TModel model, DependentBuilder<T> builder)
    {
        foreach (var step in _steps)
        {
            step(model, builder);
        }

        return builder;
    }

    private static IEnumerable<string> GetChangesWithPaths(Expression<Func<TModel, T>> expression)
    {
        var visitor = new ChangesWithExpressionVisitor();
        visitor.Visit(expression);
        var paths = visitor.Paths;
        return paths;
    }

    private class ChangesWithExpressionVisitor : ExpressionVisitor
    {
        private string _currentPath = string.Empty;
        private bool _parentUnsubscribable = false;

        public List<string> Paths { get; private set; } = new List<string>();

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                if (node.NodeType == ExpressionType.MemberAccess && node is MemberExpression memberNode)
                {
                    var member = memberNode.Member;

                    if (_parentUnsubscribable)
                    {
                        Debug.WriteLine($"Warning: cannot subscribe to changes to {member.Name}{_currentPath}. Treating it as immutable.");
                        _currentPath = "." + member.Name;
                        _parentUnsubscribable = false;
                    }
                    else
                    {
                        _currentPath = "." + member.Name + _currentPath;
                    }

                    if (!typeof(IPropertyChangingChangedHelper).IsAssignableFrom(member.DeclaringType))
                    {
                        // Parent member does not have PropertyChanging and PropertyChanged
                        _parentUnsubscribable = true;
                    }
                }
                else if (node.NodeType == ExpressionType.Parameter && node is ParameterExpression paramNode)
                {
                    if (_currentPath != string.Empty)
                    {
                        _currentPath = paramNode + _currentPath;
                        var redundant = Paths.FindIndex(x => (_currentPath + ".").StartsWith(x + "."));
                        if (redundant >= 0)
                            Paths[redundant] = _currentPath;
                        else
                            Paths.Add(_currentPath);
                        _currentPath = string.Empty;
                        _parentUnsubscribable = false;
                    }
                }
                else
                {
                    _currentPath = string.Empty;
                    _parentUnsubscribable = false;
                }
            }

            return base.Visit(node);
        }
    }
}
