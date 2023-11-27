namespace GodotEGP.Objects.Extensions;

using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using GodotEGP.Event;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Service;

public static partial class ObjectExtensions
{
	public static bool TryCast<T>(this object obj, out T result)
	{
    	if (obj is T)
    	{
        	result = (T)obj;
        	return true;
    	}

    	result = default(T);
    	return false;
	}	

	public static Dictionary<string,string> ToStringDictionary(this object obj)
	{
    	{
        	var dict = new Dictionary<string, string>();
        	if (obj != null)
        	{
        		foreach (var prop in obj.GetType().GetProperties())
        		{
                	dict.Add(prop.Name, prop.GetValue(obj).ToString());

            		// if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
            		// {
                	// 	dict.Add(prop.Name, prop.GetValue(obj).ToString());
            		// }
            		// else
            		// {
                	// 	LoggerManager.LogDebug("asd", "", "asd", prop.GetType());
                	// 	var subObj = prop.GetValue(obj);
                	// 	if (subObj != null)
                	// 	{
                	// 		Dictionary<string, string> subDict = subObj.ToStringDictionary();
                    //
                	// 		foreach (var subProp in subDict)
                	// 		{
                    // 			dict.Add($"{prop.Name}.{subProp.Key}", subProp.Value);
                	// 		}
                	// 	}
            		// }
        		}
        	}
        	return dict;
    	}
	}

	public static bool HasProperty(this object obj, string propertyName)
	{
    	return obj.GetType().GetProperty(propertyName) != null;
	}

	public static T DeepCopy<T>(this T object2Copy)
	{
    	using (var stream = new MemoryStream())
    	{
        	var serializer = new XmlSerializer(typeof(T));

        	serializer.Serialize(stream, object2Copy);
        	stream.Position = 0;
        	var objectCopy = (T)serializer.Deserialize(stream);

        	return objectCopy;
    	}
	}
}

public static partial class EventManagerObjectExtensions
{
	public static EventSubscription<T> Subscribe<T>(this object obj, Action<T> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null, string groupName = "") where T : Event
	{
		EventSubscription<T> subscription = new EventSubscription<T>(obj, callbackMethod, isHighPriority, oneshot, eventFilters, groupName);
		ServiceRegistry.Get<EventManager>().Subscribe(subscription);

		return subscription;
	}

	public static EventSubscription<T> SubscribeOwner<T>(this object obj, Action<T> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null, string groupName = "") where T : Event
	{
		EventSubscription<T> subscription = obj.Subscribe<T>(callbackMethod, isHighPriority, oneshot, eventFilters, groupName);
		subscription.Owner(obj);

		return subscription;
	}

	public static IEventSubscription<Event> Subscribe(this object obj, IEventSubscription<Event> eventSubscription)
	{
		ServiceRegistry.Get<EventManager>().Subscribe(eventSubscription);

		return eventSubscription;
	}


	public static EventSubscription<GodotSignal> SubscribeSignal(this GodotObject obj, string signalName, bool hasParams, Action<IEvent> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null, string groupName = "")
	{
		var sub = new EventSubscription<GodotSignal>(obj, callbackMethod, isHighPriority, oneshot, eventFilters, groupName);
		ServiceRegistry.Get<EventManager>().SubscribeSignal(obj, signalName, hasParams, sub);

		return sub;
	}

	public static void SubscribeSignal(this GodotObject obj, string signalName, bool hasParams, IEventSubscription<Event> eventSubscription)
	{
		ServiceRegistry.Get<EventManager>().SubscribeSignal(obj, signalName, hasParams, eventSubscription);
	}

	public static T Emit<T>(this object obj, Action<T> preinvokeHook = null) where T : Event, new()
	{
		T e = ServiceRegistry.Get<ObjectPoolService>().Get<T>().SetOwner(obj);

		if (preinvokeHook != null)
		{
			preinvokeHook(e);
		}

		e.Invoke();

		return e;
	}

	public static Event Emit(this object obj, IEvent e)
	{
		(e as Event).SetOwner(obj);
		(e as Event).Invoke();

		return (Event) e;
	}
}

public static partial class ObjectPoolServiceObjectExtensions
{
	public static void ReturnInstance(this object obj)
	{
		ServiceRegistry.Get<ObjectPoolService>().Return((dynamic) obj);
	}

	public static T CreateInstance<T>(this T obj) where T : class
	{
		return ServiceRegistry.Get<ObjectPoolService>().Get<T>();
	}
}

public static partial class NodeManagerObjectExtension
{
	public static T Node<T>(this string obj) where T : Node
	{
		return ServiceRegistry.Get<NodeManager>().GetNode<T>(obj);
	}
	public static List<T> Nodes<T>(this string obj) where T : Node
	{
		return ServiceRegistry.Get<NodeManager>().GetNodes<T>(obj);
	}

	public static void Connect(this string obj, string signalName, bool hasParams, Action<IEvent> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null)
	{
		ServiceRegistry.Get<NodeManager>().SubscribeSignal(obj, signalName, hasParams, callbackMethod, isHighPriority, oneshot, eventFilters);
	}
}


public static partial class EventSubscriptionExtensionMethods
{
	public static IEventSubscription<Event> Filters(this IEventSubscription<Event> obj, params IFilter[] filters)
	{
		foreach (IFilter filter in filters)
		{
			obj.EventFilters.Add(filter);
		}
		return obj;
	}

	public static IEventSubscription<Event> Owner(this IEventSubscription<Event> obj, object ownerObject)
	{
		obj.Filters(new OwnerObject(ownerObject));

		return obj;
	}
}

public static partial class DataBindManagerObjectExtension
{
	public static void Bind<T>(this object obj, Func<T> getterFirstCb, Action<T> setterFirstCb)
	{
		ServiceRegistry.Get<DataBindManager>().Bind<T>(obj, getterFirstCb, setterFirstCb);
	}

	public static void BindSignal<TT, T>(this string obj, string signalName, bool hasParams, Func<TT, T> getterFirstCb, Action<T> setterFirstCb, bool initialSet = false) where TT : Node
	{
		ServiceRegistry.Get<DataBindManager>().BindSignal<TT, T>(obj, signalName, hasParams, getterFirstCb, setterFirstCb, initialSet: initialSet);
	}
}
