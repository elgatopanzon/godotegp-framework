/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IScreenTransition
 * @created     : Sunday Nov 12, 2023 15:53:14 CST
 */

namespace GodotEGP.ScreenTransition;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial interface IScreenTransition
{
	void Show();
	void Shown();
	void Hide();
	void Hidden();
	void Reset();
}

