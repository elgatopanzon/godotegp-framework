namespace GodotEGP.Event.Filters;

using GodotEGP.Event.Events;

public partial interface IEventFilter
{
	bool Match(IEvent matchEvent);
}
