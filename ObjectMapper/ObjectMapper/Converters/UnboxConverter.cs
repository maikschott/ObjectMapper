using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  ///   Provides a conversion from a boxed type to its unboxed type.
  /// </summary>
  public class UnboxConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return targetType.IsValueType && (sourceType == typeof(object) || sourceType == typeof(ValueType)) ||
             sourceType == typeof(Enum) && (targetType.IsEnum || Nullable.GetUnderlyingType(targetType)?.IsEnum == true);
    }

    // example 1: object to int: out = in as int? ?? default(int);
    // example 2: object to int?: out = in as int?;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      if (Nullable.GetUnderlyingType(targetType) == null)
      {
        var nullableType = typeof(Nullable<>).MakeGenericType(targetType);
        return Expression.Coalesce(Expression.TypeAs(sourceExpression, nullableType), Expression.Default(targetType));
      }
      else
      {
        return Expression.TypeAs(sourceExpression, targetType);
      }
    }
  }
}