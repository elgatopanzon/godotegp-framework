namespace GodotEGP.Service;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;

public partial class NodeManager : Service
{
	private Dictionary<string, List<Node>> _registeredNodes = new Dictionary<string, List<Node>>();

	private Dictionary<string, List<DeferredSignalSubscription>> _deferredSignalSubscriptions = new Dictionary<string, List<DeferredSignalSubscription>>();

	public override void _Ready()
	{
		if (!GetReady())
		{
			LoggerManager.LogDebug("Setting up signals and events");

			// subscribe to Events related to nodes being added and removed with
			// high priority
			this.Subscribe<NodeAdded>(__On_EventNodeAdded, true);
			this.Subscribe<NodeRemoved>(__On_EventNodeRemoved, true);

			// connect to SceneTree node_added and node_removed signals
			GetTree().Connect("node_added", new Callable(this, "__On_Signal_node_added"));
			GetTree().Connect("node_removed", new Callable(this, "__On_Signal_node_removed"));

			// retroactively register existing scene tree nodes
			RegisterExistingNodes();

			_SetServiceReady(true);
		}
	}

	// signal callbacks for node_* events, used as rebroadcasters
	public void __On_Signal_node_added(Node nodeObj)
	{
		nodeObj.Emit<NodeAdded>((e) => e.SetNode(nodeObj));
	}
	public void __On_Signal_node_removed(Node nodeObj)
	{
		nodeObj.Emit<NodeRemoved>((e) => e.SetNode(nodeObj));
	}

	// process node added and removed Event objects
	public void __On_EventNodeAdded(IEvent eventObj)
	{
		NodeAdded e = (NodeAdded) eventObj;

		RegisterNode(e.NodeObj, GetNodeID(e.NodeObj));
	}
	public void __On_EventNodeRemoved(IEvent eventObj)
	{
		NodeRemoved e = (NodeRemoved) eventObj;
		
		DeregisterNode(e.NodeObj);
	}

	public void RegisterNode(Node node, string nodeId, bool registerGroups = true)
	{
		_registeredNodes.TryAdd(nodeId, new List<Node>());

		if (!_registeredNodes[nodeId].Contains(node))
		{
			_registeredNodes[nodeId].Add(node);

			// LoggerManager.LogDebug("Registered node", "", "node", new List<string>() {node.GetType().Name, nodeId});
		}

		if (registerGroups)
		{
			foreach (string group in node.GetGroups())
			{
				RegisterNode(node, $"group_{group}", false);
			}

			RegisterNode(node, node.GetPath(), false);
		}

		ProcessDeferredSignalSubscriptions(nodeId);
	}

	public void DeregisterNode(Node node)
	{
		foreach (KeyValuePair<string, List<Node>> nodeList in _registeredNodes)
		{
			nodeList.Value.RemoveAll((n) => {
				if (n.Equals(node))
				{
					_registeredNodes[nodeList.Key].Remove(node);

					LoggerManager.LogDebug("Deregistered node", "", "node", new List<string>() {node.GetType().Name, GetNodeID(node), nodeList.Key});
					return true;
				}

				return false;
			});
		}
	}

	public void RegisterExistingNodes()
	{
		foreach (Node existingNode in GetSceneTreeNodes())
		{
			__On_Signal_node_added(existingNode);
		}
	}

	public List<Node> GetSceneTreeNodes(Node rootNode = null, List<Node> nodesArray = null)
	{
		if (rootNode == null)
		{
			rootNode = GetTree().Root;
			nodesArray = new List<Node>();
		}

		nodesArray.Add(rootNode);

		foreach (Node childNode in rootNode.GetChildren())
		{
			GetSceneTreeNodes(childNode, nodesArray);
		}

		return nodesArray;
	}

	public string GetNodeID(Node node)
	{
		if (node.HasMeta("id"))
		{
			return (string) node.GetMeta("id");
		}
		else
		{
			return node.Name;
		}
	}

	public bool TryGetNode(string nodeId, out Node node)
	{
		node = null;

		if (_registeredNodes.TryGetValue(nodeId, out List<Node> nodes))
		{
			if (nodes.Count > 0)
			{
				node = nodes[nodes.Count - 1];
			}

			return true;
		}

		return false;
	}

	public bool TryGetNodes(string nodeId, out List<Node> nodes)
	{
		nodes = null;

		if (_registeredNodes.TryGetValue(nodeId, out List<Node> nodesList))
		{
			nodes = nodesList;

			return true;
		}

		return false;
	}

	public T GetNode<T>(string nodeId) where T : Node
	{
		if (TryGetNode(nodeId, out Node node))
		{
			return (T) node;
		}

		return null;
	}
	public List<T> GetNodes<T>(string nodeId) where T : Node
	{
		if (TryGetNodes(nodeId, out List<Node> nodes))
		{
			List<T> list = new List<T>();

			foreach (Node n in nodes)
			{
				list.Add((T) n);
			}

			return list;
		}

		return null;
	}

	public void SubscribeSignal(string nodeId, string signalName, bool hasParams, Action<IEvent> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null)
	{
		// converts params to object
		DeferredSignalSubscription sub = new DeferredSignalSubscription(nodeId, signalName, hasParams, callbackMethod, isHighPriority, oneshot, eventFilters);

		// add the object to a list, creating if needed
		if (!_deferredSignalSubscriptions.TryGetValue(nodeId, out List<DeferredSignalSubscription> subs))
		{
			subs = new List<DeferredSignalSubscription>();

			_deferredSignalSubscriptions.Add(nodeId, subs);
		}

		// add the subscription if there's no existing ones
		bool subscriptionExists = false;
		foreach (var foundSub in subs)
		{
			if (foundSub.CallbackMethod.Method.ToString() == callbackMethod.Method.ToString() && foundSub.SignalName == signalName)
			{
				subscriptionExists = true;
				break;
			}
		}
		if (!subscriptionExists)
		{
			// add the deferred signal subscription
			LoggerManager.LogDebug("Registering deferred subscription", "", "subscribe", $"{nodeId}: {sub.SignalName} {sub.GetHashCode()}");

			subs.Add(sub);
		}


		// process existing nodes to make the signal subscription
		ProcessDeferredSignalSubscriptions(nodeId);
	}

	public void ProcessDeferredSignalSubscriptions(string nodeId)
	{
		// check for any subs matching the nodeId
		if (_deferredSignalSubscriptions.TryGetValue(nodeId, out List<DeferredSignalSubscription> subs))
		{
			if (TryGetNodes(nodeId, out List<Node> nodes))
			{
				foreach (DeferredSignalSubscription sub in subs)
				{
					foreach (Node node in nodes)
					{
						if (!sub.ConnectedTo.Contains(node))
						{
							LoggerManager.LogDebug("Subscribing deferred to node signal", "", "subscribe", $"{nodeId}: {node} {sub.SignalName} {sub.GetHashCode()}");

							sub.ConnectedTo.Add(node);

							node.SubscribeSignal(sub.SignalName, sub.HasParams, sub.CallbackMethod, sub.IsHighPriority, sub.Oneshot, sub.EventFilters);		
						}
					}
				}
			}
			
		}
	}

	public partial class DeferredSignalSubscription
	{
		public string NodeId;
		public string SignalName;
		public bool HasParams;
		public Action<IEvent> CallbackMethod;
		public bool IsHighPriority;
		public bool Oneshot;
		public List<IFilter> EventFilters;
		public List<Node> ConnectedTo;

		public DeferredSignalSubscription(string nodeId, string signalName, bool hasParams, Action<IEvent> callbackMethod, bool isHighPriority, bool oneshot, List<IFilter> eventFilters)
		{
			this.NodeId = nodeId;
			this.SignalName = signalName;
			this.HasParams = hasParams;
			this.CallbackMethod = callbackMethod;
			this.IsHighPriority = isHighPriority;
			this.Oneshot = oneshot;
			this.EventFilters = eventFilters;

			this.ConnectedTo = new List<Node>();
		}
	}
}
