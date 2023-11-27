namespace GodotEGP.Event.Events;

using System;

public partial interface IEvent
{
	DateTime Created { get; set; }
	object Owner { get; set; }
	object Data { get; set; }
}
