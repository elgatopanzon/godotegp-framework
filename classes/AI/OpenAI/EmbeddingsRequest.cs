/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EmbeddingsRequest
 * @created     : Sunday Jan 21, 2024 20:29:30 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;

public partial class EmbeddingsRequest : RequestBase
{
	public object Input { get; set; }
	public string Model { get; set; }
	public string EncodingFormat { get; set; }

	public EmbeddingsRequest()
	{
		EncodingFormat = "float";
	}

	public List<string> GetInputs()
	{
		if (Input is string s)
		{
			return new List<string>() {s};
		}

		return (List<string>) Input;
	}
}

