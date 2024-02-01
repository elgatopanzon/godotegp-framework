/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : GatoGPT
 * @created     : Wednesday Jan 31, 2024 19:02:20 CST
 */

namespace GodotEGP.AI.GatoGPT;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.AI.OpenAI;

public partial class GatoGPT : OpenAI
{
	public GatoGPT(OpenAIConfig config) : base(config) 
	{
		Config = config;
	}

	// /v1/extended/tokenize
	public async Task<TokenizeResult> Tokenize(TokenizeRequest request)
	{
		var r = await GetResultObject<TokenizeResult>(await MakeRequestPost("/v1/extended/tokenize", request, false));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}
}

