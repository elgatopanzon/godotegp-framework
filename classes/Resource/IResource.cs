/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IResource
 * @created     : Saturday Nov 11, 2023 17:56:56 CST
 */

namespace GodotEGP.Resource;

using Godot;

public partial interface IResource<in T> where T : Resource
{
}

