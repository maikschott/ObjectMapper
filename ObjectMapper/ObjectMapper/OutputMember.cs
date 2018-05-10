using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masch.ObjectMapper
{
  /// <summary>
  /// Specifies the member path of the target field/property.
  /// </summary>
  public class OutputMember
  {
    public OutputMember(params MemberInfo[] members)
      : this((IReadOnlyCollection<MemberInfo>)members)
    {
    }

    public OutputMember(Expression expression)
      : this(GetMembers(expression))
    {
    }

    private OutputMember(IReadOnlyCollection<MemberInfo> members)
    {
      Members = members;
    }

    public IReadOnlyCollection<MemberInfo> Members { get; }

    public string MemberPath => string.Join(".", Members.Select(x => x.Name));

    public Expression Access { get; set; }

    public override string ToString()
    {
      var result = "x => x";
      if (Members.Any())
      {
        result += "." + MemberPath;
      }
      return result;
    }

    private static Stack<MemberInfo> GetMembers(Expression expression)
    {
      if (expression is LambdaExpression lambdaExpression) { expression = lambdaExpression.Body; }

      var result = new Stack<MemberInfo>();

      while (true)
      {
        if (expression is ParameterExpression) { break; }
        if (expression is MemberExpression memberExpr)
        {
          result.Push(memberExpr.Member);
          expression = memberExpr.Expression;
        }
        else
        {
          throw new ArgumentException(@"Expression is not a sequence of member expressions", nameof(expression));
        }
      }

      return result;
    }
  }
}