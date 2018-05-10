using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Masch.ObjectMapper.Converters;
using Masch.ObjectMapper.Hierarchical;
using Masch.ObjectMapper.Visitors;

namespace Masch.ObjectMapper
{
  /// <summary>
  /// Mapper class to map from a source type to a target type.<para />
  /// It is preferred to use the type-safe sub-class <see cref="Mapper{TIn,TOut}" />.
  /// </summary>
  public class MapperBase
  {
    private readonly Type inType;
    private readonly Type outType;

    /// <summary>
    /// Creates the mapping class for the specified source and target types.
    /// </summary>
    /// <param name="inType">Source/input type</param>
    /// <param name="outType">Target/output type</param>
    public MapperBase(Type inType, Type outType)
    {
      this.inType = inType;
      this.outType = outType;

      //IgnoreList = new List<LambdaExpression>();
      Members = new Dictionary<OutputMember, LambdaExpression>(new OutputMemberComparer());
      Converters = new List<IMapperConverter>
      {
        new FromNullableConverter(),
        new ToNullableConverter(),
        new BoxConverter(),
        new UnboxConverter(),
        new EnumConverter(),
        new ImplicitOperatorConverter(),
        new ExplicitOperatorConverter(),
        new StringToEnumConverter(),
        new ConvertibleToPrimitiveConverter(),
        new ToStringConverter(),
        new ToArrayConverter(),
        new ToListConverter(),
        new ToSetConverter(),
        new ToEnumerableConverter(),
        new ParentClassConverter()
      };
    }

    /// <summary>
    /// List of properties which should be ignored for <see cref="AutoMembers"/>.
    /// </summary>
    public IList<LambdaExpression> IgnoreList => throw new NotImplementedException();

    /// <summary>
    /// Associative list of input expressions to output fields/properties.
    /// </summary>
    public IDictionary<OutputMember, LambdaExpression> Members { get; }

    /// <summary>
    /// Priority list of converters to apply when source and target type differ.
    /// </summary>
    /// <remarks>To replace a converter with custom one, either remove the old one or insert the new converter before the old one,
    /// e.g. <c>Converters.Insert(0, newConverter);</c></remarks>
    public IList<IMapperConverter> Converters { get; }

    /// <summary>
    /// Create a mapping function between source and target type.
    /// </summary>
    /// <returns>mapping function</returns>
    public Func<object, object> Create()
    {
      var inObjExpr = Expression.Parameter(typeof(object), "obj");
      var inVarExpr = Expression.Variable(inType, "x");
      var mappingExpr = ConvertExpression(inObjExpr, outType) ?? GenerateMappingExpression(inVarExpr);

      Expression mappingAndConvertExpr = Expression.Block(new[] { inVarExpr }, Expression.Assign(inVarExpr, Expression.Convert(inObjExpr, inType)), mappingExpr);
      if (outType.IsValueType)
      {
        mappingAndConvertExpr = Expression.Convert(mappingAndConvertExpr, typeof(object));
      }

      return Expression.Lambda<Func<object, object>>(mappingAndConvertExpr, inObjExpr).Compile();
    }

    /// <summary>
    /// Configures that the specified field or property path of the source object should be mapped
    /// to the specified field/property path of the target object.
    /// </summary>
    /// <param name="inMemberPath">Field or property path of the source object</param>
    /// <param name="outMemberPath">Field/property path of the target object</param>
    /// <returns>self</returns>
    public virtual MapperBase WithMember(string inMemberPath, string outMemberPath)
    {
      Members[new OutputMember(CreateExpressionFromPath(outType, outMemberPath))] = CreateExpressionFromPath(inType, inMemberPath);
      return this;
    }

    /// <summary>
    /// Finds all convertible and mappable fields/properties between the source and target type and creates a mapping for them.<para />
    /// The searching is only done on the root level.
    /// </summary>
    /// <param name="memberMatchFunc">Custom function to define how to match properties between source and target types.<para />
    /// By default it is checked if both properties have the same and if they are of the same type or a converter between both types exists.</param>
    /// <returns>self</returns>
    public virtual MapperBase AutoMembers(Func<MemberInfo, MemberInfo, bool> memberMatchFunc = null)
    {
      if (memberMatchFunc == null) { memberMatchFunc = DefaultAutoMemberMatchFunc; }

      var inParameter = Expression.Parameter(inType);
      var outParameter = Expression.Parameter(outType);

      var inPropertiesAndFields = inType.GetProperties().Where(x => x.CanRead)
        .Concat<MemberInfo>(inType.GetFields());
      var outPropertiesAndFields = outType.GetProperties().Where(x => x.CanWrite)
        .Concat<MemberInfo>(outType.GetFields().Where(x => !x.IsInitOnly)).ToArray();

      foreach (var inputMember in inPropertiesAndFields)
      {
        foreach (var outputMember in outPropertiesAndFields)
        {
          if (memberMatchFunc(inputMember, outputMember))
          {
            var inExpression = Expression.Lambda(Expression.MakeMemberAccess(inParameter, inputMember), inParameter);
            var outExpression = Expression.Lambda(Expression.MakeMemberAccess(outParameter, outputMember), outParameter);

            Members[new OutputMember(outExpression)] = inExpression;
            break;
          }
        }
      }

      return this;
    }

    /// <summary>
    /// Configures that a path to a field or property should be ignored for <see cref="AutoMembers"/>.
    /// <seealso cref="IgnoreList"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns>self</returns>
    public virtual MapperBase Ignore(string path)
    {
      IgnoreList.Add(CreateExpressionFromPath(inType, path));
      return this;
    }

    /// <summary>
    /// Creates the mapping expression.
    /// </summary>
    /// <param name="inObjExpr">Parameter for the input object</param>
    /// <returns>Mapping expression</returns>
    protected virtual Expression GenerateMappingExpression(ParameterExpression inObjExpr)
    {
      if (outType.IsPrimitive || outType.IsEnum) // cannot map to a type without any fields or properties
      {
        return Expression.Default(outType);
      }

      var outObjExpr = Expression.Variable(outType, "out");

      var variables = new List<ParameterExpression>();
      var assignments = new List<Expression>();

      // create variables for input fields/properties
      var mappings = GetOptimizedMappings(inObjExpr, variables, assignments);

      // Build tree of output fields/properties
      var rootHierarchyItem = Hierarchical.Hierarchical.MakeHierarchical(mappings.Prepend((new OutputMember(), null)).ToArray(), x => x.Output.Members.ToArray(), path =>
      {
        var paramExpr = outObjExpr;
        Expression expr = paramExpr;
        foreach (var memberInfo in path)
        {
          expr = Expression.MakeMemberAccess(expr, memberInfo);
        }

        return (new OutputMember(expr), null);
      });

      rootHierarchyItem.Value.Output.Access = Expression.New(outType);

      assignments.Add(GenerateMappingBlockExpression(inObjExpr, rootHierarchyItem));
      return Expression.Block(variables, assignments);
    }

    protected virtual bool DefaultAutoMemberMatchFunc(MemberInfo sourceMember, MemberInfo targetMember)
    {
      if (sourceMember.Name != targetMember.Name) { return false; }

      var (sourceType, sourceIsStatic) = GetMemberType(sourceMember);
      var (targetType, _) = GetMemberType(targetMember);

      if (sourceIsStatic) { return false; }

      if (targetType.IsAssignableFrom(sourceType)) { return true; }

      return Converters.Any(converter => converter.CanConvert(sourceType, targetType));
    }

    protected virtual Expression ConvertExpression(Expression expression, Type targetType)
    {
      var sourceType = expression.Type;
      if (sourceType == targetType) { return expression; }

      foreach (var converter in Converters)
      {
        if (converter.CanConvert(sourceType, targetType))
        {
          return converter.Convert(expression, targetType);
        }
      }

      return null;
    }

    protected static bool IsWritable(MemberInfo memberInfo)
    {
      if (memberInfo is FieldInfo fieldInfo) { return !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral; }
      if (memberInfo is PropertyInfo propertyInfo) { return propertyInfo.CanWrite; }
      return false;
    }

    protected static (Type type, bool isStatic) GetMemberType(MemberInfo memberInfo)
    {
      switch (memberInfo)
      {
        case FieldInfo field:
          return (field.FieldType, field.IsStatic);
        case PropertyInfo property:
          return (property.PropertyType, property.GetGetMethod().IsStatic);
        case MethodInfo method:
          return (method.ReturnType, method.IsStatic);
        default:
          return (null, false);
      }
    }

    // creates a variable for every input field/property with sub-fields/properties
    private IEnumerable<(OutputMember Output, LambdaExpression Input)> GetOptimizedMappings(ParameterExpression inObjExpr, List<ParameterExpression> variables, List<Expression> assignments)
    {
      var memberList = new List<MemberExpression>();
      foreach (var memberLambda in Members.Values)
      {
        for (var member = memberLambda.Body; member is MemberExpression; member = ((MemberExpression)member).Expression)
        {
          memberList.Add((MemberExpression)member);
        }
      }

      var replacements = memberList
        .Select(x => x.Member)
        .Where(x => (x.MemberType & (MemberTypes.Field | MemberTypes.Property)) != 0)
        .GroupBy(x => x)
        .Select(x => (Member: x.Key, Count: x.Count()))
        .Where(x => x.Count > 1)
        .OrderByDescending(x => x.Count)
        .ToDictionary(x => x.Member, x => (Expression)Expression.Variable(GetMemberType(x.Member).type));
      var replacementVisitor = new MemberReplaceVisitor(replacements);
      variables.AddRange(replacements.Values.Cast<ParameterExpression>());
      var parameterReplacer = new ParameterReplaceVisitor(inObjExpr, Members.Values.SelectMany(x => x.Parameters).ToArray());
      assignments.AddRange(replacements.Select(x => Expression.Assign(x.Value, parameterReplacer.Visit(memberList.First(y => y.Member == x.Key)))));

      var mappings = Members.Select(x => (Output: x.Key, Input: (LambdaExpression)replacementVisitor.Visit(x.Value)));
      return mappings;
    }

    private BlockExpression GenerateMappingBlockExpression(ParameterExpression inObjExpr, IHierarchicalData<(OutputMember Output, LambdaExpression Input)> hierarchyItem)
    {
      var variables = new List<ParameterExpression>();
      var assignments = new List<Expression>();

      var parentAccessExpression = hierarchyItem.Value.Output.Access;
      ParameterExpression outObjExpr;
      if (parentAccessExpression is ParameterExpression parentAccessParameterExpression)
      {
        // target are the root element's fields/properties
        outObjExpr = parentAccessParameterExpression;
      }
      else
      {
        // target are a field's/properties element's sub-fields/properties -> put it in a variable for performance
        var varName = hierarchyItem.Parent == null ? "out" : "_" + hierarchyItem.Value.Output.Members.LastOrDefault()?.Name;
        outObjExpr = Expression.Variable(parentAccessExpression.Type, varName);
        variables.Add(outObjExpr);
        assignments.Add(Expression.Assign(outObjExpr, parentAccessExpression));
        parentAccessExpression = outObjExpr;
      }

      foreach (var child in hierarchyItem.Children)
      {
        var inMember = child.Value.Input;
        var outExpr = Expression.MakeMemberAccess(parentAccessExpression, child.Value.Output.Members.Last());

        // property is writable with reference type
        if (inMember == null && outExpr.Type.IsClass && IsWritable(outExpr.Member))
        {          
          var ctor = outExpr.Type.GetConstructor(Type.EmptyTypes);
          if (ctor != null)
          {
            // check whether value is null, and if so create it
            inMember = Expression.Lambda(Expression.Coalesce(outExpr, Expression.New(ctor)), inObjExpr);
          }
        }
        // replace parameter of input expression to the mapping functions input parameter, and
        // convert input expression to output type if types differ
        if (inMember != null)
        {
          var inMemberExpr = new ParameterReplaceVisitor(inObjExpr, inMember.Parameters.Single()).Visit(inMember.Body);
          assignments.Add(Expression.Assign(outExpr, ConvertExpression(inMemberExpr, outExpr.Type) ?? inMemberExpr));
        }

        // properties with sub-properties get their own scope
        if (child.Children.Any())
        {
          child.Value.Output.Access = outExpr;
          assignments.Add(GenerateMappingBlockExpression(inObjExpr, child));
        }
      }

      // value type must be written back for the changes to take effect
      if (outObjExpr.Type.IsValueType)
      {
        assignments.Add(Expression.Assign(hierarchyItem.Value.Output.Access, outObjExpr));
      }
      // reference type as scope result
      else
      {
        assignments.Add(outObjExpr);
      }
      return Expression.Block(variables, assignments);
    }

    private static LambdaExpression CreateExpressionFromPath(Type type, string path)
    {
      var paramExpr = Expression.Parameter(type);
      Expression expr = paramExpr;

      foreach (var memberName in path.Split('.'))
      {
        expr = Expression.PropertyOrField(expr, memberName);
      }

      return Expression.Lambda(expr, paramExpr);
    }
  }
}