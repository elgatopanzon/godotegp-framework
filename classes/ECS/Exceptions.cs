/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Exceptions
 * @created     : Saturday Apr 20, 2024 18:59:24 CST
 */

namespace GodotEGP.ECS.Exceptions;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;

public class MaxEntitiesReachedException : Exception
{
	public MaxEntitiesReachedException() { }
	public MaxEntitiesReachedException(string message) : base(message) { }
	public MaxEntitiesReachedException(string message, Exception inner) : base(message, inner) { }
	protected MaxEntitiesReachedException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ComponentExistsException : Exception
{
	public ComponentExistsException() { }
	public ComponentExistsException(string message) : base(message) { }
	public ComponentExistsException(string message, Exception inner) : base(message, inner) { }
	protected ComponentExistsException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ComponentNotFoundException : Exception
{
	public ComponentNotFoundException() { }
	public ComponentNotFoundException(string message) : base(message) { }
	public ComponentNotFoundException(string message, Exception inner) : base(message, inner) { }
	protected ComponentNotFoundException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ComponentNotRegisteredException : Exception
{
	public ComponentNotRegisteredException() { }
	public ComponentNotRegisteredException(string message) : base(message) { }
	public ComponentNotRegisteredException(string message, Exception inner) : base(message, inner) { }
	protected ComponentNotRegisteredException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class ComponentAlreadyRegisteredException : Exception
{
	public ComponentAlreadyRegisteredException() { }
	public ComponentAlreadyRegisteredException(string message) : base(message) { }
	public ComponentAlreadyRegisteredException(string message, Exception inner) : base(message, inner) { }
	protected ComponentAlreadyRegisteredException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class SystemNotRegisteredException : Exception
{
	public SystemNotRegisteredException() { }
	public SystemNotRegisteredException(string message) : base(message) { }
	public SystemNotRegisteredException(string message, Exception inner) : base(message, inner) { }
	protected SystemNotRegisteredException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}

public class SystemAlreadyRegisteredException : Exception
{
	public SystemAlreadyRegisteredException() { }
	public SystemAlreadyRegisteredException(string message) : base(message) { }
	public SystemAlreadyRegisteredException(string message, Exception inner) : base(message, inner) { }
	protected SystemAlreadyRegisteredException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}
