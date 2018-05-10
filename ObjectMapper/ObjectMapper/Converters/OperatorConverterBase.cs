using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion between two types via an implicit or explicit operator method defined in one of the types.
  /// </summary>
  public abstract class OperatorConverterBase : IMapperConverter
  {
    protected abstract string MethodName { get; }

    public bool CanConvert(Type sourceType, Type targetType)
    {
      return GetOperatorMethod(sourceType, targetType) != null;
    }

    // example: DateTime to DateTimeOffet: out = DateTimeOffset.op_implicit(in);
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      var opMethod = GetOperatorMethod(sourceExpression.Type, targetType);
      return Expression.Call(opMethod, sourceExpression);
    }

    private MethodInfo GetOperatorMethod(Type sourceType, Type targetType)
    {
      Func<MethodInfo, bool> condition = x => x.IsSpecialName && x.Name == MethodName && x.ReturnType == targetType && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { sourceType });
      return sourceType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(condition) ??
             targetType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(condition);
    }
  }
}