/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : TokenizeResult
 * @created     : Wednesday Jan 31, 2024 19:05:34 CST
 */

namespace GodotEGP.AI.GatoGPT;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.AI.OpenAI;

public partial class TokenizeResult : BaseResult
{
	public List<TokenizedString> Tokens { get; set; }
}

