using System.Collections.Generic;

namespace Masch.ObjectMapper.Hierarchical
{
  /// <summary>
  /// Interface for hierarchical nodes
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IHierarchical<out T>
  {
    T Parent { get; }
    IEnumerable<T> Children { get; }
  }

  /// <summary>
  /// Interface for hierarchical nodes with a value
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IHierarchicalData<out T> : IHierarchical<IHierarchicalData<T>>
  {
    T Value { get; }
  }
}
