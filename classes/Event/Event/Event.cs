namespace GodotEGP.Event.Events;

using System;
using System.Collections.Generic;
using Godot;
using System.ComponentModel;

using GodotEGP.Service;

using GodotEGP.Resource;
using GodotEGP.Scripting;

public partial class Event : IEvent
{
	public object Owner { get; set; }
	public object Data { get; set; }
	public DateTime Created { get; set; }
	public Exception Exception { get; set; }

	public Event()
	{
		Created = DateTime.Now;
	}
}

public class ObjectChanged : Event
{
}

static class EventExtensionMethods
{
	static public T SetOwner<T>(this T o, object owner) where T : Event
    {
		o.Owner = owner;
        return o;
    }
	static public T SetData<T>(this T o, params object[] data) where T : Event
    {
		o.Data = data;
        return o;
    }
	static public T SetException<T>(this T o, Exception ex) where T : Event
    {
		o.Exception = ex;
        return o;
    }

    static public T Invoke<T>(this T o) where T : Event
    {
		ServiceRegistry.Get<EventManager>().Emit(o);

		return o;
    }
}

public partial class ServiceRegistered : Event
{
}

public partial class ServiceDeregistered : Event
{
}

public partial class GodotSignal : Event
{
 	public string SignalName { get; set; }
 	public Variant[] SignalParams { get; set; }

}
static class SignalExtensionMethods
{
	static public T SetSignalName<T>(this T o, string signalName) where T : GodotSignal
    {
		o.SignalName = signalName;
        return o;
    }
	static public T SetSignalParams<T>(this T o, params Variant[] signalParams) where T : GodotSignal
    {
		o.SignalParams = signalParams;
        return o;
    }
}

public partial class NodeEvent : Event
{
	public Node NodeObj;
}
static class NodeExtensionMethods
{
	static public T SetNode<T>(this T o, Node node) where T : NodeEvent
    {
		o.NodeObj = node;
        return o;
    }
}

public partial class NodeAdded : NodeEvent
{
}

public partial class NodeRemoved : NodeEvent
{
}


public partial class ServiceReady : Event
{
}


public partial class BackgroundJobEvent : Event
{
	public object JobOwner;
	public DoWorkEventArgs DoWorkEventArgs;
	public ProgressChangedEventArgs ProgressChangedEventArgs;
	public RunWorkerCompletedEventArgs RunWorkerCompletedEventArgs;
}
static class EventBackgroundJobExtensionMethods
{
	static public T SetJobOwner<T>(this T o, object jobOwner) where T : BackgroundJobEvent
    {
		o.JobOwner = jobOwner;
        return o;
    }
	static public T SetDoWorkEventArgs<T>(this T o, DoWorkEventArgs e) where T : BackgroundJobEvent
    {
		o.DoWorkEventArgs = e;
        return o;
    }
	static public T SetProgressChangesEventArgs<T>(this T o, ProgressChangedEventArgs e) where T : BackgroundJobEvent
    {
		o.ProgressChangedEventArgs = e;
        return o;
    }
	static public T SetRunWorkerCompletedEventArgs<T>(this T o, RunWorkerCompletedEventArgs e) where T : BackgroundJobEvent
    {
		o.RunWorkerCompletedEventArgs = e;
        return o;
    }
}

public partial class BackgroundJobWorking : BackgroundJobEvent
{
}
public partial class BackgroundJobProgress : BackgroundJobEvent
{
}
public partial class BackgroundJobComplete : BackgroundJobEvent
{
}
public partial class BackgroundJobError : BackgroundJobEvent
{
}

public partial class DataOperationWorking : BackgroundJobEvent
{
}
public partial class DataOperationProgress : BackgroundJobEvent
{
}
public partial class DataOperationComplete : BackgroundJobEvent
{
}
public partial class DataOperationError : BackgroundJobEvent
{
}

// events for ValidatedValue<T> objects
public partial class ValidatedValueEvent : Event
{
	public object Value;
	public object PrevValue;
}

static public partial class ValidatedValueExtensions
{
	static public T SetValue<T>(this T o, object value) where T : ValidatedValueEvent
    {
        o.Value = value;
        return o;
    }

	static public T SetPrevValue<T>(this T o, object value) where T : ValidatedValueEvent
    {
        o.PrevValue = value;
        return o;
    }
}

public partial class ValidatedValueChanged : ValidatedValueEvent
{
}

public partial class ValidatedValueSet : ValidatedValueEvent
{
}

public partial class ConfigManagerLoader : BackgroundJobEvent
{
	public List<Config.Object> ConfigObjects;
	
}
static public partial class ConfigManagerLoaderExtensions
{
	static public T SetConfigObjects<T>(this T o, List<Config.Object> configObjects) where T : ConfigManagerLoader
	{
		o.ConfigObjects = configObjects;
		return o;
	}
}

public partial class ConfigManagerLoaderProgress : ConfigManagerLoader
{
}
public partial class ConfigManagerLoaderCompleted : ConfigManagerLoader
{
}
public partial class ConfigManagerLoaderError : ConfigManagerLoader
{
}

public partial class SaveDataEvent : BackgroundJobEvent
{
	public string Name;
	public Config.Object SaveData;
}
static public partial class SaveDataEventExtensions
{
	static public T SetName<T>(this T o, string name) where T : SaveDataEvent
	{
		o.Name = name;
		return o;
	}
	static public T SetSaveData<T>(this T o, Config.Object SaveData) where T : SaveDataEvent
	{
		o.SaveData = SaveData;
		return o;
	}
}

public class SaveDataLoadComplete : SaveDataEvent
{
	
}
public class SaveDataLoadError : SaveDataEvent
{
	
}
public class SaveDataCopyComplete : SaveDataEvent
{
	
}
public class SaveDataCopyError : SaveDataEvent
{
	
}
public class SaveDataMoveComplete : SaveDataEvent
{
	
}
public class SaveDataMoveError : SaveDataEvent
{
	
}
public class SaveDataRemoveComplete : SaveDataEvent
{
	
}
public class SaveDataRemoveError : SaveDataEvent
{
	
}

public partial class ResourceLoaderEvent : BackgroundJobEvent
{
	public List<LoaderQueueItem> Resources;
	
}
static public partial class ResourceLoaderEventExtensions
{
	static public T SetResources<T>(this T o, List<LoaderQueueItem> resources) where T : ResourceLoaderEvent
	{
		o.Resources = resources;
		return o;
	}
}

public partial class ResourceLoaderProgress : ResourceLoaderEvent
{
}
public partial class ResourceLoaderCompleted : ResourceLoaderEvent
{
}
public partial class ResourceLoaderError : ResourceLoaderEvent
{
}

public partial class SceneEvent : Event
{
	public string SceneId;
	public Node SceneInstance;
	
}
static public partial class SceneEventExtensions
{
	static public T SetSceneId<T>(this T o, string sceneId) where T : SceneEvent
	{
		o.SceneId = sceneId;
		return o;
	}
	static public T SetSceneInstance<T>(this T o, Node scene) where T : SceneEvent
	{
		o.SceneInstance = scene;
		return o;
	}
}

public partial class SceneLoaded : SceneEvent
{
}
public partial class SceneUnloaded : SceneEvent
{
}

public partial class ScreenTransitionEvent : Event
{
}
public partial class ScreenTransitionStarting : ScreenTransitionEvent
{
}
public partial class ScreenTransitionShowing : ScreenTransitionEvent
{
}
public partial class ScreenTransitionShown : ScreenTransitionEvent
{
}
public partial class ScreenTransitionHiding : ScreenTransitionEvent
{
}
public partial class ScreenTransitionHidden : ScreenTransitionEvent
{
}
public partial class ScreenTransitionFinished : ScreenTransitionEvent
{
}

public partial class SceneTransitionChainEvent : Event
{
}
public partial class SceneTransitionChainStarted : SceneTransitionChainEvent
{
}
public partial class SceneTransitionChainContinued : SceneTransitionChainEvent
{
}
public partial class SceneTransitionChainFinished : SceneTransitionChainEvent
{
}

public partial class ScriptInterpretterEvent : Event
{
	public ScriptResultOutput Result;
	
}
static public partial class ScriptInterpretterEventExtensions
{
	static public T SetResult<T>(this T o, ScriptResultOutput result) where T : ScriptInterpretterEvent
	{
		o.Result = result;
		return o;
	}
}

public partial class ScriptInterpretterRunning : ScriptInterpretterEvent {}
public partial class ScriptInterpretterWaiting : ScriptInterpretterEvent {}
public partial class ScriptInterpretterFinished : ScriptInterpretterEvent {}
public partial class ScriptInterpretterOutput : ScriptInterpretterEvent {}

public partial class ScriptServiceEvent : ScriptInterpretterEvent
{
	public ScriptInterpretter Interpretter;
	
}
static public partial class ScriptServiceEventExtensions
{
	static public T SetInterpretter<T>(this T o, ScriptInterpretter interpretter) where T : ScriptServiceEvent
	{
		o.Interpretter = interpretter;
		return o;
	}
}

public partial class ScriptRunning : ScriptServiceEvent {}
public partial class ScriptWaiting : ScriptServiceEvent {}
public partial class ScriptFinished : ScriptServiceEvent {}
public partial class ScriptOutput : ScriptServiceEvent {}



public partial class InputStateEvent : Event
{
	public Dictionary<StringName, ActionInputState> ActionStates;
	public Dictionary<string, JoypadState> JoypadStates;
	public MouseState MouseState;
	
}
static public partial class InputStateEventExtensions
{
	static public T SetStates<T>(this T o, Dictionary<StringName, ActionInputState> actionStates, Dictionary<string, JoypadState> joypadStates, MouseState mouseState) where T : InputStateEvent
	{
		o.ActionStates = actionStates;
		o.JoypadStates = joypadStates;
		o.MouseState = mouseState;
		return o;
	}
}

public partial class InputStateChanged : InputStateEvent {}

public partial class InputStateJoypadAvailable : InputStateEvent
{
	public string JoypadGuid = "";
}
public partial class InputStateJoypadUnavailable : InputStateJoypadAvailable {}
public partial class InputStateNoJoypadsAvailable : InputStateJoypadAvailable {}
