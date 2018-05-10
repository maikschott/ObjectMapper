using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from a nullable type to its non-nullable type.
  /// </summary>
  public class FromNullableConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return Nullable.GetUnderlyingType(sourceType) == targetType;
    }

    // example: int? to int: out = in ?? default;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return Expression.Coalesce(sourceExpression, Expression.Default(targetType));
    }
  }
}
