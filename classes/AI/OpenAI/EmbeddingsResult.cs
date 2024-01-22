/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EmbeddingsResult
 * @created     : Sunday Jan 21, 2024 20:22:13 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class EmbeddingsResult : ListBaseResult<EmbeddingResult>
{
	public string Model { get; set; }
	public EmbeddingsUsageResult Usage { get; set; }

	public EmbeddingsResult()
	{
		Object = "list";
		Data = new();
	}
}

public partial class EmbeddingResult : BaseResult
{
	public float[] Embedding { get; set; }
	public int Index { get; set; }

	public EmbeddingResult()
	{
		Object = "embedding";
		Index = 0;
	}
}
