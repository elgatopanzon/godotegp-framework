namespace GodotEGP.Event.Filter;

using GodotEGP.Event.Events;

public partial interface IFilter
{
	bool Match(IEvent matchEvent);
}
