using System;
using System.Linq;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an enumerable or single item to an array, e.g. IEnumerable&lt;T&gt; or T to T[].
  /// </summary>
  public class ToArrayConverter : ToEnumerableConverter
  {
    public override bool CanConvert(Type sourceType, Type targetType)
    {
      return targetType.IsArray && base.CanConvert(sourceType, targetType);
    }

    protected override Type GetTargetElementType(Type targetType)
    {
      return targetType.GetElementType();
    }

    // example: int to int[]: out = new[] { in };
    protected override Expression CreateFromItem(Expression sourceExpression, Type targetElementType)
    {
      return Expression.NewArrayInit(targetElementType, sourceExpression);
    }

    // example: IEnumerable<int> to int[]: out = in.ToArray();
    protected override Expression CreateFromEnumerable(Expression sourceExpression, Type targetElementType)
    {
      var toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))?.MakeGenericMethod(targetElementType) ?? throw new MissingMethodException("Enumerable.ToArray not found");
      return Expression.Call(toArrayMethod, sourceExpression);
    }
  }
}
