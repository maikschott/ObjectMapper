using System.Collections.Generic;
using System.Linq;

namespace Masch.ObjectMapper.Hierarchical
{
  public class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
  {
    public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
    {
      if (ReferenceEquals(x, y)) { return true; }
      if ((x == null) ^ (y == null)) { return false; }
      return x.SequenceEqual(y);
    }

    public int GetHashCode(IEnumerable<T> obj)
    {
      int result = 13;
      unchecked
      {
        foreach (var item in obj)
        {
          result = result * 397 + item.GetHashCode();
        }
      }
      return result;
    }
  }
}
