using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Masch.ObjectMapper.Converters
{
  /// <summary>
  /// Provides a conversion from an string interpretation of an enum to to the actual enum value.
  /// </summary>
  public class StringToEnumConverter : IMapperConverter
  {
    public bool CanConvert(Type sourceType, Type targetType)
    {
      return sourceType == typeof(string) && targetType.IsEnum;
    }

    // example: string to BindingFlags: Enum.TryParse<BindingFlags>(in, true, out var out) ? out : default(BindingFlags);
    public Expression Convert(Expression sourceExpression, Type targetType)
    {
      var enumParseMethod = GetType().GetMethod(nameof(TryParseEnum), BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(targetType) ?? throw new MissingMethodException("TryParseEnum not found");
      return Expression.Call(enumParseMethod, sourceExpression);
    }

    private static TEnum TryParseEnum<TEnum>(string value) where TEnum: struct
    {
      return Enum.TryParse<TEnum>(value, true, out var result) ? result : default(TEnum);
    }
  }
}
