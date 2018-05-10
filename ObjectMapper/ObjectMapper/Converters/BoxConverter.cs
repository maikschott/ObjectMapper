using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  ///   Provides a conversion from a value type to a box type.
  /// </summary>
  public class BoxConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType.IsValueType && (targetType == typeof(object) || targetType == typeof(ValueType)) ||
             targetType == typeof(Enum) && (sourceType.IsEnum || Nullable.GetUnderlyingType(sourceType)?.IsEnum == true);
    }

    // example: int to object: out = (object)in;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return Expression.Convert(sourceExpression, targetType);
    }
  }
}