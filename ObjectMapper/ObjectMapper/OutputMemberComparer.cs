using System.Collections.Generic;
using System.Reflection;
using Masch.ObjectMapper.Hierarchical;

namespace Masch.ObjectMapper
{
  internal class OutputMemberComparer : IEqualityComparer<OutputMember>
  {
    private readonly EnumerableEqualityComparer<MemberInfo> memberComparer;

    public OutputMemberComparer()
    {
      memberComparer = new EnumerableEqualityComparer<MemberInfo>();
    }

    public bool Equals(OutputMember x, OutputMember y)
    {
      return memberComparer.Equals(x?.Members, y?.Members);
    }

    public int GetHashCode(OutputMember obj)
    {
      return obj.Members == null ? 0 : memberComparer.GetHashCode(obj.Members);
    }
  }
}