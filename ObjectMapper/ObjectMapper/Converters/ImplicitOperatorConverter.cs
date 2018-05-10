namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion between two types via an implicit operator method defined in one of the types.
  /// </summary>
  public class ImplicitOperatorConverter : OperatorConverterBase
  {
    protected override string MethodName => "op_Implicit";
  }
}