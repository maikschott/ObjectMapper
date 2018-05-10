using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from a class to its parent class.
  /// </summary>
  public class ParentClassConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType.IsClass && targetType.IsAssignableFrom(sourceType);
    }

    // example: CultureInfo to object: out = in;
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return sourceExpression;
    }
  }
}
