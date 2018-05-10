namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion between two types via an explicit operator method defined in one of the types.
  /// </summary>
  public class ExplicitOperatorConverter : OperatorConverterBase
  {
    protected override string MethodName => "op_Explicit";
  }
}