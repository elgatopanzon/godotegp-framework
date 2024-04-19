namespace GodotEGP.Event.Filters;

using GodotEGP.Event.Events;
using GodotEGP.Objects.ObjectPool;

public partial interface IEventFilter : IPoolableObject
{
	public bool Enabled { get; set; }
	bool Match(IEvent matchEvent);
}
