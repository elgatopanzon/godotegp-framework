/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Exceptions
 * @created     : Friday Mar 29, 2024 22:55:49 CST
 */

namespace GodotEGP.Chainables.Exceptions;

using System;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public class ChainableStreamBufferMergeException : Exception
{
	public ChainableStreamBufferMergeException() { }
	public ChainableStreamBufferMergeException(string message) : base(message) { }
	public ChainableStreamBufferMergeException(string message, Exception inner) : base(message, inner) { }
	protected ChainableStreamBufferMergeException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableOutputSchemaTypeException : Exception
{
	public ChainableOutputSchemaTypeException() { }
	public ChainableOutputSchemaTypeException(string message) : base(message) { }
	public ChainableOutputSchemaTypeException(string message, Exception inner) : base(message, inner) { }
	protected ChainableOutputSchemaTypeException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableInputSchemaTypeException : Exception
{
	public ChainableInputSchemaTypeException() { }
	public ChainableInputSchemaTypeException(string message) : base(message) { }
	public ChainableInputSchemaTypeException(string message, Exception inner) : base(message, inner) { }
	protected ChainableInputSchemaTypeException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableChainSchemaMismatchException : Exception
{
	public ChainableChainSchemaMismatchException() { }
	public ChainableChainSchemaMismatchException(string message) : base(message) { }
	public ChainableChainSchemaMismatchException(string message, Exception inner) : base(message, inner) { }
	protected ChainableChainSchemaMismatchException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableConfigParamsTypeMismatchException : Exception
{
	public ChainableConfigParamsTypeMismatchException() { }
	public ChainableConfigParamsTypeMismatchException(string message) : base(message) { }
	public ChainableConfigParamsTypeMismatchException(string message, Exception inner) : base(message, inner) { }
	protected ChainableConfigParamsTypeMismatchException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableConfigParamsNonExistantParameterException : Exception
{
	public ChainableConfigParamsNonExistantParameterException() { }
	public ChainableConfigParamsNonExistantParameterException(string message) : base(message) { }
	public ChainableConfigParamsNonExistantParameterException(string message, Exception inner) : base(message, inner) { }
	protected ChainableConfigParamsNonExistantParameterException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableStreamBufferMismatchingTypesException : Exception
{
	public ChainableStreamBufferMismatchingTypesException() { }
	public ChainableStreamBufferMismatchingTypesException(string message) : base(message) { }
	public ChainableStreamBufferMismatchingTypesException(string message, Exception inner) : base(message, inner) { }
	protected ChainableStreamBufferMismatchingTypesException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableStreamBufferMergeNotSupported : Exception
{
	public ChainableStreamBufferMergeNotSupported() { }
	public ChainableStreamBufferMergeNotSupported(string message) : base(message) { }
	public ChainableStreamBufferMergeNotSupported(string message, Exception inner) : base(message, inner) { }
	protected ChainableStreamBufferMergeNotSupported(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ChainableStreamBufferMergeEmpty : Exception
{
	public ChainableStreamBufferMergeEmpty() { }
	public ChainableStreamBufferMergeEmpty(string message) : base(message) { }
	public ChainableStreamBufferMergeEmpty(string message, Exception inner) : base(message, inner) { }
	protected ChainableStreamBufferMergeEmpty(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}
