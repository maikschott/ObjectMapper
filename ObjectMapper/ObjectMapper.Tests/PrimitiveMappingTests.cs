using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Masch.ObjectMapper.Tests
{
  [TestClass]
  public class PrimitiveMappingTests
  {
    private const int TestValue = 1;
    private const BindingFlags TestEnum = BindingFlags.Public;
    private const BindingFlags TestMultiEnum = BindingFlags.Public | BindingFlags.Static;
    private static readonly CultureInfo testReference = CultureInfo.InvariantCulture;
    private static readonly IEnumerable<int> testEnumerable = Enumerable.Range(1, 10);

    [TestMethod]
    public void ValueTypeMapping_Ident_ReturnEqualValue()
    {
      Mapper<int, int>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ReferenceTypeMapping_Ident_ReturnSameValue()
    {
      Mapper<CultureInfo, CultureInfo>.Default(testReference).Should().BeSameAs(testReference);
    }

    [TestMethod]
    public void BoxConverter_BoxToObject_ShouldReturnBoxedValue()
    {
      Mapper<int, object>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void BoxConverter_BoxToValueType_ShouldReturnBoxedValue()
    {
      Mapper<int, ValueType>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void BoxConverter_BoxToEnumType_ShouldReturnBoxedValue()
    {
      Mapper<BindingFlags, Enum>.Default(TestEnum).Should().Be(TestEnum);
    }

    [TestMethod]
    public void BoxConverter_BoxNullableToEnumType_ShouldReturnBoxedValue()
    {
      Mapper<BindingFlags?, Enum>.Default(TestEnum).Should().Be(TestEnum);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToByte_ShouldSucceed()
    {
      Mapper<int, byte>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToSByte_ShouldSucceed()
    {
      Mapper<int, sbyte>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToShort_ShouldSucceed()
    {
      Mapper<int, short>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToUShort_ShouldSucceed()
    {
      Mapper<int, ushort>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToInt_ShouldSucceed()
    {
      Mapper<byte, int>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToUInt_ShouldSucceed()
    {
      Mapper<int, uint>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToLong_ShouldSucceed()
    {
      Mapper<int, long>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToULong_ShouldSucceed()
    {
      Mapper<int, ulong>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToFloat_ShouldSucceed()
    {
      Mapper<int, float>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToDouble_ShouldSucceed()
    {
      Mapper<int, double>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToDecimal_ShouldSucceed()
    {
      Mapper<int, decimal>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToBool_ShouldSucceed()
    {
      Mapper<int, bool>.Default(TestValue).Should().Be(TestValue != 0);
    }

    [TestMethod]
    public void ConvertibleConverter_ConvertToChar_ShouldSucceed()
    {
      Mapper<int, char>.Default(TestValue).Should().Be((char)TestValue);
    }

    [TestMethod]
    public void EnumConverter_ConvertToEnum_ShouldReturnEnum()
    {
      Mapper<int, BindingFlags>.Default(TestValue).Should().Be((BindingFlags)TestValue);
    }

    [TestMethod]
    public void EnumConverter_ConvertToUnderlyingType_ShouldReturnUnderlyingType()
    {
      Mapper<BindingFlags, int>.Default(TestEnum).Should().Be((int)TestEnum);
    }

    [TestMethod]
    public void FromNullableConverter_ConvertWithValue_ShouldReturnValue()
    {
      Mapper<int?, int>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void FromNullableConverter_ConvertWithNull_ShouldReturnDefaultValue()
    {
      Mapper<int?, int>.Default(null).Should().Be(default(int));
    }

    [TestMethod]
    public void ToNullableConverter_ConvertToNullable_ShouldReturnValue()
    {
      Mapper<int, int?>.Default(TestValue).Should().Be(TestValue);
    }

    [TestMethod]
    public void StringToEnumConverter_ConvertSingleEnumValue_ShouldReturnEnum()
    {
      Mapper<string, BindingFlags>.Default(TestEnum.ToString()).Should().Be(TestEnum);
    }

    [TestMethod]
    public void StringToEnumConverter_ConvertMultiEnumValue_ShouldReturnEnum()
    {
      Mapper<string, BindingFlags>.Default(TestMultiEnum.ToString()).Should().Be(TestMultiEnum);
    }

    [TestMethod]
    public void ParentClassConverter_ConvertReferenceToObject_ShouldReturnSame()
    {
      Mapper<CultureInfo, object>.Default(testReference).Should().BeSameAs(testReference);
    }

    [TestMethod]
    public void ToArrayConverter_ConvertSingleValue_ShouldReturnArray()
    {
      Mapper<int, int[]>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToArrayConverter_ConvertEnumerable_ShouldReturnArray()
    {
      Mapper<IEnumerable<int>, int[]>.Default(testEnumerable).Should().BeEquivalentTo(testEnumerable);
    }

    [TestMethod]
    public void ToEnumerableConverter_ConvertSingleValue_ShouldReturnEnumerable()
    {
      Mapper<int, IEnumerable<int>>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToEnumerableConverter_ConvertEnumerable_ShouldReturnEnumerable()
    {
      var testArray = testEnumerable.ToArray();
      Mapper<int[], IEnumerable<int>>.Default(testArray).Should().BeSameAs(testArray);
    }

    [TestMethod]
    public void ToListConverter_ConvertSingleValueToInterface_ShouldReturnList()
    {
      Mapper<int, IList<int>>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToListConverter_ConvertSingleValueToClass_ShouldReturnList()
    {
      Mapper<int, List<int>>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToListConverter_ConvertEnumerableToInterface_ShouldReturnList()
    {
      Mapper<IEnumerable<int>, IList<int>>.Default(testEnumerable).Should().BeEquivalentTo(testEnumerable);
    }

    [TestMethod]
    public void ToListConverter_ConvertEnumerableToClass_ShouldReturnList()
    {
      Mapper<IEnumerable<int>, List<int>>.Default(testEnumerable).Should().BeEquivalentTo(testEnumerable);
    }

    [TestMethod]
    public void ToSetConverter_ConvertSingleValueToInterface_ShouldReturnSet()
    {
      Mapper<int, ISet<int>>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToSetConverter_ConvertSingleValueToClass_ShouldReturnSet()
    {
      Mapper<int, HashSet<int>>.Default(TestValue).Should().BeEquivalentTo(new[] { TestValue });
    }

    [TestMethod]
    public void ToSetConverter_ConvertEnumerableToInterface_ShouldReturnList()
    {
      Mapper<IEnumerable<int>, ISet<int>>.Default(testEnumerable).Should().BeEquivalentTo(testEnumerable);
    }

    [TestMethod]
    public void ToSetConverter_ConvertEnumerableToClass_ShouldReturnList()
    {
      Mapper<IEnumerable<int>, HashSet<int>>.Default(testEnumerable).Should().BeEquivalentTo(testEnumerable);
    }

    [TestMethod]
    public void ToStringConverter_ConvertValue_ShouldReturnToString()
    {
      Mapper<int, string>.Default(TestValue).Should().Be(TestValue.ToString());
    }

    [TestMethod]
    public void ToStringConverter_ConvertReference_ShouldReturnToString()
    {
      Mapper<CultureInfo, string>.Default(testReference).Should().Be(testReference.ToString());
    }

    [TestMethod]
    public void ToStringConverter_ConvertNullReference_ShouldReturnNull()
    {
      Mapper<CultureInfo, string>.Default(null).Should().BeNull();
    }

    [TestMethod]
    public void ToStringConverter_ConvertMultiEnum_ShouldReturnToString()
    {
      Mapper<BindingFlags, string>.Default(TestMultiEnum).Should().Be(TestMultiEnum.ToString());
    }
  }
}
