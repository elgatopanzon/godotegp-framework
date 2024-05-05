/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Exceptions
 * @created     : Wednesday May 01, 2024 22:05:08 CST
 */

namespace GodotEGP.ECSv3.Exceptions;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;

public class OperationOnDeadEntityException : Exception
{
	public OperationOnDeadEntityException() { }
	public OperationOnDeadEntityException(string message) : base(message) { }
	public OperationOnDeadEntityException(string message, Exception inner) : base(message, inner) { }
	protected OperationOnDeadEntityException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}
