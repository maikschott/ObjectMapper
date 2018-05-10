using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an object to a string.
  /// </summary>
  public class ToStringConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType != typeof(string) && targetType == typeof(string);
    }

    // example: int to string: out = in.ToString();
    // example: CultureInfo to string: out = in == null ? null : in.ToString();
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      Expression result = Expression.Call(sourceExpression, typeof(object).GetMethod(nameof(ToString)) ?? throw new MissingMethodException("ToString not found"));
      if (sourceExpression.Type.IsClass)
      {
        result = Expression.Condition(Expression.ReferenceEqual(sourceExpression, Expression.Constant(null, sourceExpression.Type)), Expression.Constant(null, result.Type), result);
      }
      return result;
    }
  }
}
