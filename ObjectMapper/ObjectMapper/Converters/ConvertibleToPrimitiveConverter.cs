using System;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from a type implementing <see cref="IConvertible"/> to a type supported by this interface.
  /// </summary>
  public class ConvertibleToPrimitiveConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return typeof(IConvertible).IsAssignableFrom(sourceType) && targetType.IsPrimitive && targetType != typeof(IntPtr) && targetType != typeof(UIntPtr);
    }

    // example: IConvertible to int (primitive type): out = (int)Convert.ChangeType((object)in, typeof(int));
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      return Expression.Unbox(
        Expression.Call(
          typeof(Convert).GetMethod(nameof(System.Convert.ChangeType), new[] { typeof(object), typeof(Type) }) ?? throw new MissingMethodException("Convert.ChangeType not found"),
          Expression.Convert(sourceExpression, typeof(object)), Expression.Constant(targetType)),
        targetType);
    }
  }
}
