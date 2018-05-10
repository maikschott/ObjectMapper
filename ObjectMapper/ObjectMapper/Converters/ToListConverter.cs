using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an enumerable or single item to a list, e.g. IEnumerable&lt;T&gt; or T to List&lt;T&gt;.
  /// </summary>
  public class ToListConverter : ToEnumerableConverter
  {
    protected virtual Type ListTemplateInterface => typeof(IList<>);

    protected virtual Type ListTemplateType => typeof(List<>);

    public override bool CanConvert(Type sourceType, Type targetType)
    {
      if (!targetType.IsGenericType) { return false; }
      var typeDefinition = targetType.GetGenericTypeDefinition();

      return (typeDefinition == ListTemplateType || typeDefinition == ListTemplateInterface) && base.CanConvert(sourceType, targetType);
    }

    // example: int to List<int>: out = new List<int> { in };
    protected override Expression CreateFromItem(Expression sourceExpression, Type targetElementType)
    {
      var listType = ListTemplateType.MakeGenericType(targetElementType);
      var ctor = listType.GetConstructor(Type.EmptyTypes) ?? throw new MissingMethodException("Default list constructor not found");
      return Expression.ListInit(Expression.New(ctor), sourceExpression);
    }

    // example: IEnumerable<int> to List<int>: out = new List<int>(in);
    protected override Expression CreateFromEnumerable(Expression sourceExpression, Type targetElementType)
    {
      var listType = ListTemplateType.MakeGenericType(targetElementType);
      var ctor = listType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(targetElementType) }) ?? throw new MissingMethodException("List constructor with enumerable argument not found");
      return Expression.New(ctor, sourceExpression);
    }
  }
}
