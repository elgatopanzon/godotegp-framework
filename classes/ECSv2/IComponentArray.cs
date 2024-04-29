/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IComponentArray
 * @created     : Sunday Apr 28, 2024 16:45:42 CST
 */

namespace GodotEGP.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial interface IComponentArray
{
	public void DestroyComponents(Entity entity);
}
