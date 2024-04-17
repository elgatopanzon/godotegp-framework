/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RemoteTransferResult
 * @created     : Friday Feb 02, 2024 17:00:02 CST
 */

namespace GodotEGP.Resource.Resources;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.DAL.Endpoints;

using System;

public partial class RemoteTransferResult : Godot.Resource
{
	public FileEndpoint FileEndpoint { get; set; }
	public HTTPEndpoint HTTPEndpoint { get; set; }
	public int TransferType { get; set; }
	public long BytesTransferred { get; set; }
	public TimeSpan TransferTime { get; set; }
	public double TransferSpeed { get; set; }
}

