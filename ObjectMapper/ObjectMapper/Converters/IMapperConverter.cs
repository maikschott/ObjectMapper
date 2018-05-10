using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Interface for mapping converters
  /// </summary>
  /// <seealso cref="MapperBase.Converters"/>
  public interface IMapperConverter
  {
    /// <summary>
    /// Can this converter convert from <see cref="sourceType"/> to <see cref="targetType"/>?
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="targetType">Target type</param>
    /// <returns></returns>
    bool CanConvert(Type sourceType, Type targetType);

    /// <summary>
    /// Convert expression of source type to target type.
    /// </summary>
    /// <param name="sourceExpression">Source expression</param>
    /// <param name="targetType">Target type</param>
    /// <returns>Target expression, i.e. source expression with conversion to target type</returns>
    Expression Convert(Expression sourceExpression, Type targetType);
  }
}
