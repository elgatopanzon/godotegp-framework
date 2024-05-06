/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IQueryFilter
 * @created     : Friday May 03, 2024 13:54:56 CST
 */

namespace GodotEGP.ECSv3.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3;

public partial interface IQueryFilter
{
	public FilterMatchType MatchType { get; }
	public Entity Entity { get; set; }
	public Query ScopedQuery { get; set; }
	public bool TriggerFilterEnd { get; set; }
	public FilterMatchMethod MatchMethod { get; set; }
}
