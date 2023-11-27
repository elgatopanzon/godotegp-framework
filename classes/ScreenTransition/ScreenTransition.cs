/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScreenTransition
 * @created     : Sunday Nov 12, 2023 15:57:07 CST
 */

namespace GodotEGP.ScreenTransition;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ScreenTransition : Node, IScreenTransition
{
	// called by the ScreenTransitionManager to show the transition
	public virtual void Show()
	{
		this.Emit<ScreenTransitionShowing>();

		_OnShow();
	}

	public virtual void Shown()
	{
		_OnShown();

		this.Emit<ScreenTransitionShown>();
	}

	// called by the ScreenTransitionManager to hide the transition
	public virtual void Hide()
	{
		this.Emit<ScreenTransitionHiding>();

		_OnHide();
	}

	public virtual void Hidden()
	{
		_OnHidden();

		this.Emit<ScreenTransitionHidden>();
	}

	public virtual void Reset()
	{
		_OnReset();
	}

	// override these for implementing the transition states
	public virtual void _OnShow()
	{
		Shown();
	}
	public virtual void _OnShown()
	{
	}
	public virtual void _OnHide()
	{
		Hidden();
	}
	public virtual void _OnHidden()
	{
	}
	public virtual void _OnReset()
	{
	}
}

