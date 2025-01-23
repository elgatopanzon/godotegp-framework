/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ISystem
 * @created     : Wednesday May 08, 2024 15:13:15 CST
 */

namespace GodotEGP.ECSv4.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Queries;

public partial interface ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query);
}
