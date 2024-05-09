/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Entity
 * @created     : Friday Apr 26, 2024 20:06:09 CST
 */

namespace GodotEGP.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Runtime.InteropServices;
using System.Numerics;

using GodotEGP.ECSv3.Components;

// entity struct holds the entity ID and a reference to the ECS core
[StructLayout(LayoutKind.Explicit)]
public struct Entity : IIncrementOperators<Entity>, IEquatable<Entity>, IEquatable<ulong>
{
	// underlying ulong ID of this entity
	[FieldOffset(0)]
	internal readonly ulong _id;
	[FieldOffset(0)]
	public readonly ulong RawId;

	// uint version of this id (left-side ID)
	[FieldOffset(0)]
	public uint Id;

	// version of this entity
	[FieldOffset(4)]
	public ushort Version;

	// unused 2nd version of this entity
	[FieldOffset(6)]
	public ushort Version2;

	// right side pair ID
	[FieldOffset(4)]
	public uint PairId;

	public Entity(ulong id)
	{
		_id = id;
	}
	public Entity(uint id, uint id2)
	{
		Id = id;
		PairId = id2;
	}

	/**********************
	*  Equality methods  *
	**********************/

	public override bool Equals(object? entity)
	{
		return ((Entity) entity).RawId == RawId; // 1400 fps
		// Entity? v = entity as Entity?; 
		// return v.Value.RawId == RawId; // 666 fps
	}
	public bool Equals(Entity entity)
	{
		return RawId == entity.RawId;
	}
	public bool Equals(ulong entity)
	{
		return RawId == entity;
	}
	public override int GetHashCode()
	{
		return RawId.GetHashCode();
	}

	public static Entity CreateFrom(ulong id)
	{
		return new Entity(id);
	}

	public static Entity CreateFrom(uint id, uint id2)
	{
		return new Entity(id, id2);
	}

	public override string ToString()
	{
		return $"Entity {_id}: ({Id}, {PairId})";
	}

	/************************
	*  Operator overloads  *
	************************/

	public static Entity operator ++(Entity other)
	{
		other.Version++;
		return other;
	}
	
	public static implicit operator ulong(Entity entity) => entity.RawId;
	public static implicit operator Entity(ulong id) => Entity.CreateFrom(id);


	/********************************
	*  Entity ID encoding methods  *
	********************************/

	// get the left-side ID from a ulong ID
	public static uint GetEncodedId(ulong id)
	{
		return (uint) id;
	}
	
	// get the entity version from a ulong ID
	public static ushort GetEncodedVersion(ulong id)
	{
		return (ushort) (id >> 32);
	}

	// set the entity version on a ulong ID
	public static ulong SetEncodedVersion(ulong id, ushort version)
	{
		return (ulong) ((ulong) version << 32) | GetEncodedId(id);
	}

	// increment the entity version on a ulong ID
	public static ulong IncrementVersion(ulong id)
	{
		return SetEncodedVersion(id, (ushort) (GetEncodedVersion(id) + (ushort) 1));
	}

	// set the right side pair ID on a ulong ID
	public static ulong SetPairId(ulong id, ulong idLeft)
	{
		return (ulong) ((ulong) GetEncodedId(idLeft) << 32) | GetEncodedId(id);
	}

	// get the right side pair ID of a ulong ID
	public static ulong GetPairId(ulong id)
	{
		return (uint) (id >> 32);
	}
}
