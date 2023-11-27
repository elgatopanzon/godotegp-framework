namespace GodotEGP.Data.Operation;
using System;

using System.Collections.Generic;
using System.Net.Http;

using GodotEGP.Data.Endpoint;
using GodotEGP.Event.Events;
using GodotEGP.Objects.Extensions;

public partial class OperationProcess<T>
{
	protected object _dataObject;

	public Operation<T> DataOperation;

	public OperationProcess(Operation<T> dataOperation, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		DataOperation = dataOperation;

		// subscribe to event subscriptions
		if (onWorkingCb != null)
		{
			DataOperation.SubscribeOwner<DataOperationWorking>(onWorkingCb, oneshot: true, isHighPriority: true);
		}
		if (onProgressCb != null)
		{
			DataOperation.SubscribeOwner<DataOperationProgress>(onProgressCb, oneshot: true, isHighPriority: true);
		}
		if (onCompleteCb != null)
		{
			DataOperation.SubscribeOwner<DataOperationComplete>(onCompleteCb, oneshot: true, isHighPriority: true);
		}
		if (onErrorCb != null)
		{
			DataOperation.SubscribeOwner<DataOperationError>(onErrorCb, oneshot: true, isHighPriority: true);
		}
	}

	public void Load()
	{
		DataOperation.Load();
	}
	public void Save()
	{
		DataOperation.Save();
	}
}

public partial class DataOperationProcessFile<T> : OperationProcess<T>
{
	public DataOperationProcessFile(string filePath, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new FileOperation<T>(new FileEndpoint(filePath), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessFile(FileEndpoint fileEndpoint, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new FileOperation<T>(fileEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}

public partial class DataOperationProcessHTTP<T> : OperationProcess<T>
{
	public DataOperationProcessHTTP(string hostname, int port = 443, string path = "/", Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new HTTPOperation<T>(new HTTPEndpoint(hostname, port, path, urlParams, requestMethod, verifySSL, timeout, headers), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessHTTP(HTTPEndpoint httpEndpoint, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new HTTPOperation<T>(httpEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}
