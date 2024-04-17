/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IChainableAssign
 * @created     : Tuesday Apr 02, 2024 23:45:51 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;
using System.Threading.Tasks;

public partial interface IChainableAssign : IChainable
{
	public Dictionary<string, object> AssignedOutputs { get; set; }

	public Task<Dictionary<string, object>> Run(Dictionary<string, object> input = null);
	public Task<Dictionary<string, object>[]> Batch(Dictionary<string, object>[] batchInput = null);
	public IAsyncEnumerable<Dictionary<string, object>> Stream(Dictionary<string, object> input = null);
}
