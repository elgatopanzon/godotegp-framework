/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Entity
 * @created     : Friday Apr 26, 2024 20:06:09 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Runtime.InteropServices;
using System.Numerics;

using GodotEGP.ECSv4.Components;

// entity struct holds the entity ID and a reference to the ECS core
public struct Entity : IEquatable<Entity>, IEquatable<int>, IComparable<Entity>
{
	public int Id;
	public int RawId
	{
		get {
			return Id;
		}
	}

	public Entity(int id)
	{
		Id = id;
	}

	/**********************
	*  Equality methods  *
	**********************/

	public override bool Equals(object? entity)
	{
		return ((Entity) entity).Id == Id;
	}
	public bool Equals(Entity entity)
	{
		return RawId == entity.RawId;
	}
	public bool Equals(int entity)
	{
		return RawId == entity;
	}
	public override int GetHashCode()
	{
		return RawId;
	}

	public int CompareTo(Entity entity)
	{
		return RawId.CompareTo(entity.RawId);
	}

	public static Entity CreateFrom(int id)
	{
		return new Entity(id);
	}

	public override string ToString()
	{
		return $"Entity {Id}";
	}

	/************************
	*  Operator overloads  *
	************************/

	public static implicit operator int(Entity entity) {
		return entity.Id;
	}


	/********************************
	*  Entity ID encoding methods  *
	********************************/

	// // get the left-side ID from a ulong ID
	// public static uint GetEncodedId(ulong id)
	// {
	// 	return (uint) id;
	// }
	//
	// // get the entity version from a ulong ID
	// public static ushort GetEncodedVersion(ulong id)
	// {
	// 	return (ushort) (id >> 32);
	// }
    //
	// // set the entity version on a ulong ID
	// public static ulong SetEncodedVersion(ulong id, ushort version)
	// {
	// 	return (ulong) ((ulong) version << 32) | GetEncodedId(id);
	// }
    //
	// // increment the entity version on a ulong ID
	// public static ulong IncrementVersion(ulong id)
	// {
	// 	return SetEncodedVersion(id, (ushort) (GetEncodedVersion(id) + (ushort) 1));
	// }
    //
	// // set the right side pair ID on a ulong ID
	// public static ulong SetPairId(ulong id, ulong idLeft)
	// {
	// 	return (ulong) ((ulong) GetEncodedId(idLeft) << 32) | GetEncodedId(id);
	// }
    //
	// // get the right side pair ID of a ulong ID
	// public static uint GetPairId(ulong id)
	// {
	// 	return (uint) (id >> 32);
	// }
}
