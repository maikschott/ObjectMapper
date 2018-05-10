using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an enum type to its underlying type and vice versa.
  /// </summary>
  public class EnumConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType.IsEnum && targetType == sourceType.GetEnumUnderlyingType() ||
             targetType.IsEnum && sourceType == targetType.GetEnumUnderlyingType();
    }

    // example: int to BindingFlags: out = (BindingFlags)in;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return Expression.Convert(sourceExpression, targetType);
    }
  }
}