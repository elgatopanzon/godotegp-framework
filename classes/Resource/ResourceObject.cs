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

using GodotEGP.DAL.Endpoints;

public abstract partial class ResourceObjectBase
{
	public abstract object RawValue {get;}
	public static object Create(Type parameterType)
    {
        Type genericType = typeof(ResourceObject<>).MakeGenericType(parameterType);
        return Activator.CreateInstance(genericType);
    }

    public abstract string Category {get; set;}
    public abstract string Id {get; set;}
    public abstract ResourceDefinition Definition {get; set;}
}

public partial class ResourceObject<T> : ResourceObjectBase, IResourceObject<T> where T : Godot.Resource
{
	public T Value;

	public override object RawValue {
		get {
			return Value;
		}
	}

	public override string Category { get; set; }
	public override string Id { get; set; }
	public override ResourceDefinition Definition { get; set; }

	public ResourceObject()
	{
		
	}
}

