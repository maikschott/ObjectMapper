using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Masch.ObjectMapper.Tests
{
  [TestClass]
  public class MappingTests
  {
    private Mapper<CultureInfo, CultureInfoDto> mapper;
    private readonly CultureInfo testdata = CultureInfo.InvariantCulture;

    [TestMethod]
    public void Mapper_CultureInfoAutoMember_ShouldMapAllConvertibleProperties()
    {
      mapper.AutoMembers();
      var dto = mapper.Create()(testdata);

      dto.Calendar.Should().Be(testdata.Calendar.ToString());
      dto.CultureTypes.Should().Be((int)testdata.CultureTypes);
      dto.DisplayName.Should().Be(testdata.DisplayName);
      dto.Name.Should().Be(testdata.Name);
      dto.LocalName.Should().BeNull();
    }

    [TestMethod]
    public void Mapper_CultureInfoAutoMemberWithNameOverride_ShouldIncludeUserDefinedMapping()
    {
      var map = mapper
        .AutoMembers((src, dst) => src.Name == dst.Name || src.Name == nameof(CultureInfo.NativeName) && dst.Name == nameof(CultureInfoDto.LocalName))
        .Create();
      var dto = map(testdata);

      dto.LocalName.Should().Be(testdata.NativeName);
    }

    [TestMethod]
    public void Mapper_CustomMappingWithFunctionCall_ShouldSucceed()
    {
      var map = mapper
        .WithMember(ci => Guid.NewGuid(), x => x.Id)
        .Create();
      var dto = map(testdata);

      dto.Id.Should().NotBeEmpty();
    }

    [TestMethod]
    public void Mapper_CustomMappingWithGetterOnlyReference_ShouldSucceed()
    {
      var map = mapper
        .WithMember(ci => ci.NumberFormat.NativeDigits, x => x.NumericGetOnly.Digits)
        .Create();
      var dto = map(testdata);

      dto.NumericGetOnly.Digits.Should().BeEquivalentTo(testdata.NumberFormat.NativeDigits);
    }

    [TestMethod]
    public void Mapper_CustomMappingWithReference_ShouldMaintainReference()
    {
      var map = mapper
        .WithMember(ci => ci.NumberFormat.NativeDigits, x => x.NumericDefault.Digits)
        .Create();
      var dto = map(testdata);

      dto.NumericDefault.Should().NotBeNull();
      dto.NumericDefault.Digits.Should().BeEquivalentTo(testdata.NumberFormat.NativeDigits);
      dto.NumericDefaultSetterCount.Should().Be(1); // from the constructor
    }

    [TestMethod]
    public void Mapper_CustomMappingWithNullReference_ShouldCreateReference()
    {
      var map = mapper
        .WithMember(ci => ci.NumberFormat.NativeDigits, x => x.NumericNull.Digits)
        .Create();
      var dto = map(testdata);

      dto.NumericNull.Should().NotBeNull();
      dto.NumericNull.Digits.Should().BeEquivalentTo(testdata.NumberFormat.NativeDigits);
    }

    [TestMethod]
    public void Mapper_CustomMappingWithStructProperty_ShouldMaintainContents()
    {
      var map = mapper
        .WithMember(ci => ci.NumberFormat.PercentSymbol, x => x.Percent.Sign)
        .Create();
      var dto = map(testdata);

      dto.Percent.Sign.Should().BeEquivalentTo(testdata.NumberFormat.PercentSymbol);
    }

    [TestMethod]
    public void Mapper_CustomMappingDeep_ShouldSucceed()
    {
      var map = mapper
        .WithMember(ci => ci.NumberFormat.PercentSymbol, x => x.NumericDefault.Percent.Sign)
        .Create();
      var dto = map(testdata);

      dto.NumericDefault.Percent.Sign.Should().BeEquivalentTo(testdata.NumberFormat.PercentSymbol);
    }

    [TestMethod]
    public void Mapper_NonGeneric_ShouldSucceed()
    {
      var nonGenericMapper = new MapperBase(typeof(CultureInfo), typeof(CultureInfoDto));
      var map = nonGenericMapper
        .WithMember("NumberFormat.PercentSymbol", "NumericDefault.Percent.Sign")
        .Create();
      var dto = (CultureInfoDto)map(testdata);

      dto.NumericDefault.Percent.Sign.Should().BeEquivalentTo(testdata.NumberFormat.PercentSymbol);
    }

    [TestInitialize]
    public void Setup()
    {
      mapper = new Mapper<CultureInfo, CultureInfoDto>();
    }
  }
}
