using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Masch.ObjectMapper
{
  /// <summary>
  ///   Mapper class to map from a source type to a target type.
  /// </summary>
  /// <typeparam name="TIn">Source/input type</typeparam>
  /// <typeparam name="TOut">Target/output type</typeparam>
  public class Mapper<TIn, TOut> : MapperBase
  {
    private static readonly Lazy<Func<TIn, TOut>> defaultMapper = new Lazy<Func<TIn, TOut>>(() => new Mapper<TIn, TOut>().AutoMembers().Create());

    /// <summary>
    /// Returns a default mapper configured with the <see cref="AutoMembers"/> option.<para />
    /// The mapper is cached so that it is only created the first time this property is called and then reused for every subsequent call.
    /// </summary>
    public static Func<TIn, TOut> Default => defaultMapper.Value;

    public Mapper()
      : base(typeof(TIn), typeof(TOut))
    {
    }

    /// <inheritdoc cref="MapperBase.Create" />
    public new Func<TIn, TOut> Create()
    {
      var inObjExpr = Expression.Parameter(typeof(TIn), "x");
      var mappingExpr = ConvertExpression(inObjExpr, typeof(TOut)) ?? GenerateMappingExpression(inObjExpr);
      return Expression.Lambda<Func<TIn, TOut>>(mappingExpr, inObjExpr).Compile();
    }

    /// <summary>
    ///   Configures that the specified field or property path of the source object should be mapped
    ///   to the specified field/property path of the target object.
    ///   <para />
    ///   Source and target types must be the same.
    /// </summary>
    /// <param name="inMember">Input expression of arbitrary complexity</param>
    /// <param name="outMember">Output expression specifying a field/property path</param>
    /// <returns>self</returns>
    public virtual Mapper<TIn, TOut> WithMember<TValue>(Expression<Func<TIn, TValue>> inMember, Expression<Func<TOut, TValue>> outMember)
    {
      Members[new OutputMember(outMember)] = inMember;
      return this;
    }

    /// <summary>
    ///   Configures that the specified field or property path of the source object should be mapped
    ///   to the specified field/property path of the target object.
    ///   <para />
    ///   If the source and target types differ, <see cref="MapperBase.Converters" /> is searched to find a fitting converter
    ///   to
    ///   convert between source and target.
    /// </summary>
    /// <param name="inMember">Input expression of arbitrary complexity</param>
    /// <param name="outMember">Output expression specifying a field/property path</param>
    /// <returns>self</returns>
    public virtual Mapper<TIn, TOut> ConvertMember<TInValue, TOutValue>(Expression<Func<TIn, TInValue>> inMember, Expression<Func<TOut, TOutValue>> outMember)
    {
      Members[new OutputMember(outMember)] = inMember;
      return this;
    }

    /// <inheritdoc cref="MapperBase.AutoMembers" />
    public new virtual Mapper<TIn, TOut> AutoMembers(Func<MemberInfo, MemberInfo, bool> memberMatchFunc = null)
    {
      base.AutoMembers(memberMatchFunc);
      return this;
    }

    /// <summary>
    ///   Configures that a field or property should be ignored for <see cref="AutoMembers" />.
    ///   <seealso cref="MapperBase.IgnoreList" />
    /// </summary>
    /// <param name="member"></param>
    /// <returns>self</returns>
    public virtual Mapper<TIn, TOut> Ignore<TValue>(Expression<Func<TIn, TValue>> member)
    {
      IgnoreList.Add(member);
      return this;
    }
  }
}