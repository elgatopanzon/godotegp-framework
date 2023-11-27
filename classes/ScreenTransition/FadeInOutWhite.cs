/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : FadeInOutWhite
 * @created     : Sunday Nov 12, 2023 19:31:38 CST
 */

namespace GodotEGP.ScreenTransition;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class FadeInOutWhite : ScreenTransition
{
	// called by the ScreenTransitionManager to show the transition
	// override these for implementing the transition states
	public override void _Ready()
	{
		"ScreenTransition.FadeInOutWhite.Anim".Connect(AnimationPlayer.SignalName.AnimationFinished, true, _On_AnimationPlayer_animation_finished, isHighPriority: true);
	}

	public void _On_AnimationPlayer_animation_finished(IEvent e)
	{
		if (e is GodotSignal es)
		{
			string animName = (string) es.SignalParams[0];

			LoggerManager.LogDebug("Transition animation player finished", "", "e", animName);	

			if (animName == "show")
			{
				Shown();
			}
			if (animName == "hide")
			{
				Hidden();
			}
		}
	}

	public override void _OnShow()
	{
		"ScreenTransition.FadeInOutWhite.Canvas".Node<CanvasLayer>().Visible = true;
		"ScreenTransition.FadeInOutWhite.Anim".Node<AnimationPlayer>().Play("show");
	}
	public override void _OnShown()
	{
	}
	public override void _OnHide()
	{
		LoggerManager.LogDebug("Playing hide animation");
		"ScreenTransition.FadeInOutWhite.Anim".Node<AnimationPlayer>().Play("hide");
	}
	public override void _OnHidden()
	{
		LoggerManager.LogDebug("Stopping animation");
		"ScreenTransition.FadeInOutWhite.Canvas".Node<CanvasLayer>().Visible = false;
		"ScreenTransition.FadeInOutWhite.Anim".Node<AnimationPlayer>().Stop();
	}
	public override void _OnReset()
	{
	}
}

