using System;
using System.Collections.Generic;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an enumerable or single item to a set, e.g. IEnumerable&lt;T&gt; or T to HashSet&lt;T&gt;.
  /// </summary>
  public class ToSetConverter : ToListConverter
  {
    protected override Type ListTemplateInterface => typeof(ISet<>);

    protected override Type ListTemplateType => typeof(HashSet<>);
  }
}