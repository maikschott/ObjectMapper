# ObjectMapper
A simple to use object to object mapper

## Setting up a mapping configuration
A mapping configuration is created by instantiating the Mapper class:
```c#
var mapper = new Mapping<SourceType, TargetType>();
````

## Setting up the member mappings
To map from a member of the source class to a member of the target class, use the **WithMember** method:

```c#
mapper.WithMember(src => src.Property, dst => dest.Property);
```

The source expression can be arbitrary complexity, to include method calls, conversions, and so on.

The target expression must be chain of properties and fields.

_WithMember_ requires that the types of both expressions match.
If they don't match either include a conversion in the source expression
or use the **ConvertMember** method which provides a set of automatic type conversions, such as:
- boxing a value type into the _object_ or _ValueType_ base types
- boxing an enum member into the _Enum_ base type
- unboxing from _object_, _ValueType_, _Enum_ to a value type
- convert a value type to Nullable, e.g. _int_ to _int?_
- convert a Nullable type to its contained value type, e.g. _int?_ to _int_
- convert a reference type to a parent type
- convert an enum to its underlying type and vice versa, e.g. _BindingFlags_ to _int_
- convert a string representing a (flagged) enum to an (flagged) enum
- convert any type to a string
- convert a type implementing _IConvertible_ to a primitive type
- convert an enumerable to an array
- convert an enumerable to a list
- convert an enumerable to a set
- convert to any type if either the source class or the target class contains an implicit or explicit conversion operator.

The mapper configuration can be called with **AutoMembers** function, to automatically provide a mapping between each member of the source and target class having the same name and there existing a matching conversion.

Each method of the mapping configuration can be chained, so that a mapping is as simple as:

```c#
public class CultureInfoDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string DisplayName { get; set; }
  public string Calendar { get; set; }
  public string CalendarType { get; set; }
}

mapper = new Mapper<CultureInfo, CultureInfoDto>()
	.AutoMembers()
	.WithMember(ci => Guid.NewGuid(), dto => dto.Id)
	.WithMember(ci => ci.Calendar.AlgorithmType.ToString(), dto => dto.CalendarType);
```

## Using a mapping
To use the mapping call **Create** to retrieve the conversion delegate:

```c#
var mappingFunc = mapper.Create();
var src = CultureInfo.CurrentCulture; // example
var dst = mappingFunc(src);
```

_dst_ is now a mapped copy of the source object, i.e. _dst_ is of type _CultureInfoDto_ and its properties contain the same values the source object had.
In this example:
- dst.Id contains a random GUID because of _WithMember(ci => Guid.NewGuid(), dto => dto.Id)_,
- dst.Name and DisplayName have the same values of src.Name and src.DisplayName, because they have the same name and type,
- dst.Calendar is the string representation of src.Calendar because properties have the same name and their exists an between both types,
- dst.CalendarType has the ToString value of src.Calendar.AlgorithmType because of _WithMember(ci => ci.Calendar.AlgorithmType.ToString(), dto => dto.CalendarType)_.

## Non-generic class
If the source and target type are not known at compile time, the non-generic **MapperBase** base class can be used.

Its _WithMember_ function takes strings representing field/property pathes, but does not allow complex mappings. However, types are automatically converted.
```c#
mapper = new MapperBase(typeof(CultureInfo), typeof(CultureInfoDto))
	.AutoMembers()
	.WithMember("Calendar.AlgorithmType", "CalendarType").
```
