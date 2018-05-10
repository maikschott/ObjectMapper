using System;
using System.Collections.Generic;
using System.Linq;

namespace Masch.ObjectMapper.Hierarchical
{
  public static class Hierarchical
  {
    /// <summary>
    /// Transforms a flat collection with children to a hierarchical data structure.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <typeparam name="TId">Hierarchical reference value type</typeparam>
    /// <param name="collection">Flat collection</param>
    /// <param name="pathSelector">Selects the property holding the nodes children</param>
    /// <param name="missingItemFunc">Callback to create missing items in the tree</param>
    /// <returns>Root item of the created hierarchical collection</returns>
    public static IHierarchicalData<T> MakeHierarchical<T, TId>(ICollection<T> collection, Func<T, IList<TId>> pathSelector, Func<IEnumerable<TId>, T> missingItemFunc)
    {
      var enumerableEqualityComparer = new EnumerableEqualityComparer<TId>();
      var ids = new Dictionary<IList<TId>, (int id, int parentId)>(enumerableEqualityComparer);
      var existingPathes = new HashSet<IList<TId>>(enumerableEqualityComparer);

      foreach (var item in collection)
      {
        var path = pathSelector(item);
        existingPathes.Add(path);
        if (path.Any())
        {
          AddHierarchicalPath(path);
        }
        else if (!ids.ContainsKey(path))
        {
          ids.Add(path, (0, 0));
        }
      }

      var newPathes = ids.Keys.Except(existingPathes, enumerableEqualityComparer);
      var missingItems = newPathes.Select(missingItemFunc);

      return MakeHierarchical(collection.Concat(missingItems), x => ids[pathSelector(x)].id, x => ids[pathSelector(x)].parentId);

      int AddHierarchicalPath(IList<TId> path)
      {
        if (!path.Any()) { return 0; }
        if (ids.TryGetValue(path, out var idPair)) { return idPair.id; }

        var parentPath = path.Take(path.Count - 1).ToArray();
        var parentId = AddHierarchicalPath(parentPath);

        var id = ids.Count + 1;
        ids.Add(path, (id, parentId));
        return id;
      }
    }

    public static IHierarchicalData<T> MakeHierarchical<T, TId>(IEnumerable<T> enumerable, Func<T, TId> idSelector, Func<T, TId> parentIdSelector, TId rootId = default(TId))
    {
      var dict = enumerable.ToDictionary(idSelector, x => new HierarchicalData<T>(x));
      if (!dict.TryGetValue(rootId, out var root))
      {
        root = new HierarchicalData<T>(default(T));
        dict.Add(rootId, root);
      }

      foreach (var item in dict.Values.Where(x => x != root))
      {
        dict[parentIdSelector(item.Value)].Add(item);
      }

      return root;
    }

    /// <summary>
    /// Hierarchical node
    /// </summary>
    /// <typeparam name="T">Node value type</typeparam>
    private class HierarchicalData<T> : IHierarchicalData<T>
    {
      private readonly IList<IHierarchicalData<T>> children;

      public HierarchicalData(T value)
      {
        Value = value;
        children = new List<IHierarchicalData<T>>();
      }

      public T Value { get; }

      public IHierarchicalData<T> Parent { get; private set; }

      public IEnumerable<IHierarchicalData<T>> Children => children;

      public void Add(HierarchicalData<T> child)
      {
        children.Add(child);
        child.Parent = this;
      }

      public override string ToString()
      {
        return Value.ToString();
      }
    }
  }
}