﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ICSharpCode.NRefactory.TypeSystem
{
	#if WITH_CONTRACTS
	[ContractClass(typeof(ITypeContract))]
	#endif
	public interface IType : ITypeReference, INamedElement, IEquatable<IType>
	{
		/// <summary>
		/// Gets the type kind.
		/// </summary>
		TypeKind Kind { get; }
		
		/// <summary>
		/// Gets whether the type is a reference type or value type.
		/// </summary>
		/// <returns>
		/// true, if the type is a reference type.
		/// false, if the type is a value type.
		/// null, if the type is not known (e.g. unconstrained generic type parameter or type not found)
		/// </returns>
		/// <remarks>
		/// The resolve context is required for type parameters with a constraint "T : SomeType":
		/// the type parameter is a reference type iff SomeType is a class type.
		/// </remarks>
		bool? IsReferenceType(ITypeResolveContext context);
		
		/// <summary>
		/// Gets the underlying type definition.
		/// Can return null for types which do not have a type definition (for example arrays, pointers, type parameters)
		/// </summary>
		ITypeDefinition GetDefinition();
		
		/// <summary>
		/// Gets the parent type, if this is a nested type.
		/// Returns null for top-level types.
		/// </summary>
		IType DeclaringType { get; }
		
		/// <summary>
		/// Gets the number of type parameters.
		/// </summary>
		int TypeParameterCount { get; }
		
		/// <summary>
		/// Calls ITypeVisitor.Visit for this type.
		/// </summary>
		/// <returns>The return value of the ITypeVisitor.Visit call</returns>
		IType AcceptVisitor(TypeVisitor visitor);
		
		/// <summary>
		/// Calls ITypeVisitor.Visit for all children of this type, and reconstructs this type with the children based
		/// on the return values of the visit calls.
		/// </summary>
		/// <returns>A copy of this type, with all children replaced by the return value of the corresponding visitor call.
		/// If the visitor returned the original types for all children (or if there are no children), returns <c>this</c>.
		/// </returns>
		IType VisitChildren(TypeVisitor visitor);
		
		/// <summary>
		/// Gets the direct base types.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <returns>Returns the direct base types including interfaces</returns>
		IEnumerable<IType> GetBaseTypes(ITypeResolveContext context);
		
		/// <summary>
		/// Gets inner classes (including inherited inner classes).
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which types to return.
		/// The filter is tested on the original type definitions (before parameterization).</param>
		/// <remarks>
		/// If the nested type is generic (and has more type parameters than the outer class),
		/// this method will return a parameterized type,
		/// where the additional type parameters are set to <see cref="SharedType.UnboundTypeArgument"/>.
		/// 
		/// Type parameters belonging to the outer class will have the value copied from the outer type
		/// if it is a parameterized type. Otherwise, those existing type parameters will be self-parameterized,
		/// and thus 'leaked' to the caller in the same way the GetMembers() method does not specialize members
		/// from an <see cref="ITypeDefinition"/> and 'leaks' type parameters in member signatures.
		/// </remarks>
		/// <example>
		/// <code>
		/// class Base&lt;T> {
		/// 	class Nested&lt;X> {}
		/// }
		/// class Derived&lt;A, B> : Base&lt;B> {}
		/// 
		/// Derived[string,int].GetNestedTypes() = { Base`1+Nested`1[int, unbound] }
		/// Derived.GetNestedTypes() = { Base`1+Nested`1[`1, unbound] }
		/// Base[`1].GetNestedTypes() = { Base`1+Nested`1[`1, unbound] }
		/// Base.GetNestedTypes() = { Base`1+Nested`1[`0, unbound] }
		/// </code>
		/// </example>
		IEnumerable<IType> GetNestedTypes(ITypeResolveContext context, Predicate<ITypeDefinition> filter = null);
		
		// Note that we cannot 'leak' the additional type parameter as we leak the normal type parameters, because
		// the index might collide. For example,
		//   class Base<T> { class Nested<X> {} }
		//   class Derived<A, B> : Base<B> { }
		// 
		// Derived<string, int>.GetNestedTypes() = Base+Nested<int, UnboundTypeArgument>
		// Derived.GetNestedTypes() = Base+Nested<`1, >
		//  Here `1 refers to B, and there's no way to return X as it would collide with B.
		
		/// <summary>
		/// Gets all instance constructors for this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which constructors to return.
		/// The filter is tested on the original method definitions (before specialization).</param>
		/// <remarks>
		/// This list does not include constructors in base classes or static constructors.
		/// For methods on parameterized types, type substitution will be performed on the method signature,
		/// and the appriopriate <see cref="SpecializedMethod"/> will be returned.
		/// </remarks>
		IEnumerable<IMethod> GetConstructors(ITypeResolveContext context, Predicate<IMethod> filter = null);
		
		/// <summary>
		/// Gets all methods that can be called on this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which methods to return.
		/// The filter is tested on the original method definitions (before specialization).</param>
		/// <remarks>
		/// The list does not include constructors.
		/// For methods on parameterized types, type substitution will be performed on the method signature,
		/// and the appriopriate <see cref="SpecializedMethod"/> will be returned.
		/// </remarks>
		IEnumerable<IMethod> GetMethods(ITypeResolveContext context, Predicate<IMethod> filter = null);
		
		/// <summary>
		/// Gets all properties that can be called on this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which properties to return.
		/// The filter is tested on the original property definitions (before specialization).</param>
		/// <remarks>
		/// For properties on parameterized types, type substitution will be performed on the property signature,
		/// and the appriopriate <see cref="SpecializedProperty"/> will be returned.
		/// </remarks>
		IEnumerable<IProperty> GetProperties(ITypeResolveContext context, Predicate<IProperty> filter = null);
		
		/// <summary>
		/// Gets all fields that can be accessed on this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which constructors to return.
		/// The filter is tested on the original field definitions (before specialization).</param>
		/// <remarks>
		/// For fields on parameterized types, type substitution will be performed on the field's return type,
		/// and the appriopriate <see cref="SpecializedField"/> will be returned.
		/// </remarks>
		IEnumerable<IField> GetFields(ITypeResolveContext context, Predicate<IField> filter = null);
		
		/// <summary>
		/// Gets all events that can be accessed on this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which events to return.
		/// The filter is tested on the original event definitions (before specialization).</param>
		/// <remarks>
		/// For fields on parameterized types, type substitution will be performed on the event's return type,
		/// and the appriopriate <see cref="SpecializedEvent"/> will be returned.
		/// </remarks>
		IEnumerable<IEvent> GetEvents(ITypeResolveContext context, Predicate<IEvent> filter = null);
		
		/// <summary>
		/// Gets all members that can be called on this type.
		/// </summary>
		/// <param name="context">The context used for resolving type references</param>
		/// <param name="filter">The filter used to select which members to return.
		/// The filter is tested on the original member definitions (before specialization).</param>
		/// <remarks>
		/// The resulting list is the union of GetFields(), GetProperties(), GetMethods() and GetEvents().
		/// It does not include constructors.
		/// For parameterized types, type substitution will be performed.
		/// </remarks>
		IEnumerable<IMember> GetMembers(ITypeResolveContext context, Predicate<IMember> filter = null);
	}
	
	#if WITH_CONTRACTS
	[ContractClassFor(typeof(IType))]
	abstract class ITypeContract : ITypeReferenceContract, IType
	{
		bool? IType.IsReferenceType(ITypeResolveContext context)
		{
			Contract.Requires(context != null);
			return null;
		}
		
		int IType.TypeParameterCount {
			get {
				Contract.Ensures(Contract.Result<int>() >= 0);
				return 0;
			}
		}
		
		IType IType.DeclaringType {
			get { return null; }
		}
		
		IEnumerable<IType> IType.GetBaseTypes(ITypeResolveContext context)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IType>>() != null);
			return null;
		}
		
		IEnumerable<IType> IType.GetNestedTypes(ITypeResolveContext context, Predicate<ITypeDefinition> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IType>>() != null);
			return null;
		}

		IEnumerable<IMethod> IType.GetMethods(ITypeResolveContext context, Predicate<IMethod> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IMethod>>() != null);
			return null;
		}
		
		IEnumerable<IMethod> IType.GetConstructors(ITypeResolveContext context, Predicate<IMethod> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IMethod>>() != null);
			return null;
		}
		
		IEnumerable<IProperty> IType.GetProperties(ITypeResolveContext context, Predicate<IProperty> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IProperty>>() != null);
			return null;
		}
		
		IEnumerable<IField> IType.GetFields(ITypeResolveContext context, Predicate<IField> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IField>>() != null);
			return null;
		}
		
		IEnumerable<IEvent> IType.GetEvents(ITypeResolveContext context, Predicate<IEvent> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IEvent>>() != null);
			return null;
		}
		
		IEnumerable<IMember> IType.GetEvents(ITypeResolveContext context, Predicate<IMember> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IMember>>() != null);
			return null;
		}
		
		string INamedElement.FullName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.Name {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.Namespace {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.ReflectionName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		ITypeDefinition IType.GetDefinition()
		{
			return null;
		}
		
		bool IEquatable<IType>.Equals(IType other)
		{
			return false;
		}
		
		IType IType.AcceptVisitor(TypeVisitor visitor)
		{
			Contract.Requires(visitor != null);
			Contract.Ensures(Contract.Result<IType>() != null);
			return this;
		}
		
		IType IType.VisitChildren(TypeVisitor visitor)
		{
			Contract.Requires(visitor != null);
			Contract.Ensures(Contract.Result<IType>() != null);
			return this;
		}
	}
	#endif
}
