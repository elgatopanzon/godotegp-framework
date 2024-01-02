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
using GodotEGP.Resource;

using GodotEGP.Data.Endpoint;

public abstract partial class ResourceBase
{
	public abstract object RawValue {get;}
	public static object Create(Type parameterType)
    {
        Type genericType = typeof(Resource<>).MakeGenericType(parameterType);
        return Activator.CreateInstance(genericType);
    }

    public abstract string Category {get; set;}
    public abstract string Id {get; set;}
    public abstract Definition Definition {get; set;}
}

public partial class Resource<T> : ResourceBase, IResource<T> where T : Resource
{
	public T Value;

	public override object RawValue {
		get {
			return Value;
		}
	}

	public override string Category { get; set; }
	public override string Id { get; set; }
	public override Definition Definition { get; set; }

	public Resource()
	{
		
	}
}

