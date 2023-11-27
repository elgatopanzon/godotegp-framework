/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Resource
 * @created     : Saturday Nov 11, 2023 17:55:09 CST
 */

namespace GodotEGP.Resource;

using System;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Data.Endpoint;

public abstract partial class ResourceBase
{
	public abstract object RawValue {get;}
	public static object Create(Type parameterType)
    {
        Type genericType = typeof(Resource<>).MakeGenericType(parameterType);
        return Activator.CreateInstance(genericType);
    }
}

public partial class Resource<T> : ResourceBase, IResource<T> where T : Resource
{
	public T Value;

	public override object RawValue {
		get {
			return Value;
		}
	}

	public Resource()
	{
		
	}
}

