using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an enumerable subtype or single item to an enumerable, e.g. IList&lt;T&gt; or T to IEnumerable&lt;T&gt;.
  /// </summary>
  public class ToEnumerableConverter : IMapperConverter
  {
    public virtual bool CanConvert(Type sourceType, Type targetType)
    {
      var targetElementType = GetTargetElementType(targetType);
      if (targetElementType == null) { return false; }

      var sourceElementType = GetEnumerableType(sourceType) ?? sourceType;
      if (sourceElementType == typeof(void)) { sourceElementType = typeof(object); } // Quelle ist lediglich IEnumerable

      return sourceElementType == typeof(object) || targetElementType.IsAssignableFrom(sourceElementType);
    }

    public virtual Expression Convert(Expression sourceExpression, Type targetType)
    {
      var sourceElementType = GetEnumerableType(sourceExpression.Type);
      var targetElementType = GetTargetElementType(targetType) ?? throw new ArgumentException(@"Invalid target type", nameof(targetType));

      if (sourceElementType == null)
      {
        if (targetElementType != sourceExpression.Type)
        {
          sourceExpression = Expression.Convert(sourceExpression, targetElementType);
        }
        return CreateFromItem(sourceExpression, targetElementType);
      }
      if (sourceElementType != targetElementType)
      {
        var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(targetElementType) ?? throw new MissingMethodException("Enumerable.Cast not found");
        sourceExpression = Expression.Call(castMethod, sourceExpression);
      }

      return CreateFromEnumerable(sourceExpression, targetElementType);
    }

    protected virtual Type GetTargetElementType(Type targetType)
    {
      return GetEnumerableType(targetType);
    }

    // example: int to IEnumerable<int>: out = Enumerable.Repeat(in, 1);
    protected virtual Expression CreateFromItem(Expression sourceExpression, Type targetElementType)
    {
      var repeatMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Repeat))?.MakeGenericMethod(targetElementType) ?? throw new MissingMethodException("Enumerable.Repeat not found");
      return Expression.Call(repeatMethod, sourceExpression, Expression.Constant(1));
    }

    // example: List<int> to IEnumerable<int>: out = in;
    protected virtual Expression CreateFromEnumerable(Expression sourceExpression, Type targetElementType)
    {
      return sourceExpression;
    }

    protected static Type GetEnumerableType(Type enumerableType)
    {
      if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
      {
        var genericEnumerable = enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? enumerableType : enumerableType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return genericEnumerable?.GetGenericArguments()[0] ?? typeof(void);
      }

      return null;
    }
  }
}