using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from a non-nullable type to its nullable type.
  /// </summary>
  public class ToNullableConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType == Nullable.GetUnderlyingType(targetType);
    }

    // example: int to int?: out = (int?)in;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return Expression.Convert(sourceExpression, targetType);
    }
  }
}
