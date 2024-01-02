namespace GodotEGP;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.Logging;
using GodotEGP.Service;

public partial class ServiceRegistry : Node
{
	// Static ServiceRegistry instance
	public static ServiceRegistry Instance { get; private set; }

	// Dictionary of BaseService objects
	private Dictionary<Type, Service.Service> _serviceObjs = new Dictionary<Type, Service.Service>();

	/// <summary>
	/// Access service objects using []
	/// <example>
	/// <code>
	/// ServiceRegistry.Instance[Service]
	/// </code>
	/// </example>
	/// </summary>
	public Service.Service this[Type serviceType] {
		get {
			return GetService(serviceType);
		}
	}

	public ServiceRegistry() 
	{
		Instance = this;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoggerManager.LogDebug("Subscribing to ServiceReady events");

		// Get<EventManager>().Subscribe(new EventSubscription<EventServiceReady>(this, __On_EventServiceReady, true));
		this.Subscribe<ServiceReady>(__On_EventServiceReady, isHighPriority:false); // switch to low priority to allow startup code to execute
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Register a <c>BaseService</c> object to the registry, with short-form
	/// name
	/// <param name="serviceObj">Instance of a BaseService object</param>
	/// <param name="serviceName">Short-name for the service object</param>
	/// </summary>
	public void RegisterService(Service.Service serviceObj)
	{
		_serviceObjs.Add(serviceObj.GetType(), serviceObj);

		AddChild(serviceObj);

		LoggerManager.LogDebug($"Service registered!", "", "service", serviceObj.GetType().Name);

		serviceObj._OnServiceRegistered();

		// Get<EventManager>().Emit(new EventServiceRegistered().SetOwner(serviceObj));
		serviceObj.Emit<ServiceRegistered>();
	}

	public void __On_EventServiceReady(IEvent eventObj)
	{
		if (eventObj.Owner.TryCast(out Service.Service s))
		{
			LoggerManager.LogDebug($"Service ready!", "", "service", s.GetType().Name);

			s._OnServiceReady();
		}
	}

	/// <summary>
	/// Get a service object
	/// <param name="serviceType">Short-name of the service object</param>
	/// </summary>
	public Service.Service GetService(Type serviceType)
	{
		if (_serviceObjs.ContainsKey(serviceType))
			return _serviceObjs[serviceType];

		return null;
	}

	/// <summary>
	/// Get a service object by the given type
	/// </summary>
	public static T Get<T>() where T : Service.Service, new()
	{
		if (!Instance._serviceObjs.TryGetValue(typeof(T), out Service.Service obj))
		{
			LoggerManager.LogDebug("Lazy-creating service instance", "", "service", typeof(T).Name);

			obj = new T();
			Instance.RegisterService(obj);
		}

		return (T) obj;
	}

	/// <summary>
	/// Get a service's ready state
	/// </summary>
	public static bool IsServiceReady<T>() where T : Service.Service, new()
	{
		return Get<T>().GetReady();
	}

	public static bool WaitForServices(params Type[] p)
	{
		bool servicesReady = false;

		while (true)
		{
			int serviceReadyCount = 0;

			foreach (Type serviceType in p)
			{
				foreach (var serviceObj in Instance._serviceObjs)
				{
					if (serviceObj.Key == serviceType && serviceObj.Value.GetReady())
					{
						serviceReadyCount++;
					}
				}
			}

			LoggerManager.LogDebug($"Waiting for services... {serviceReadyCount}/{p.Count()}", "", "services", p.Select(e => e.Name));

			if (serviceReadyCount == p.Count())
			{
				servicesReady = true;
				break;
			}
			else
			{
				// wait 100ms before checking again
				System.Threading.Thread.Sleep(100);

				// process low-priority event queue
				ServiceRegistry.Get<EventManager>()._Process(0);
			}
		}

		return servicesReady;
	}
}
