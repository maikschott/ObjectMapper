using System;
using System.Linq;
using System.Linq.Expressions;

namespace Masch.ObjectMapper.Visitors
{
  public class ParameterReplaceVisitor : ExpressionVisitor
  {
    private readonly Expression replacement;
    private readonly ParameterExpression[] parameters;

    public ParameterReplaceVisitor(Expression replacement, params ParameterExpression[] parameters)
    {
      this.replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
      this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      return parameters.Contains(node) ? replacement : base.VisitParameter(node);
    }
  }
}
