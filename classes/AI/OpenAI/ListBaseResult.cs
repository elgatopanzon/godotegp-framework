/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ListBaseResult
 * @created     : Sunday Jan 21, 2024 20:25:35 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ListBaseResult<T> : BaseResult where T : BaseResult
{
	public List<T> Data { get; set; }
}
