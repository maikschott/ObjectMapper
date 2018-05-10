using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Masch.ObjectMapper.Visitors
{
  public class MemberReplaceVisitor : ExpressionVisitor
  {
    private readonly Dictionary<MemberInfo, Expression> replacements;

    public MemberReplaceVisitor(Dictionary<MemberInfo, Expression> replacements)
    {
      this.replacements = replacements;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (replacements.TryGetValue(node.Member, out var replacement))
      {
        return replacement;
      }

      return base.VisitMember(node);
    }
  }
}
