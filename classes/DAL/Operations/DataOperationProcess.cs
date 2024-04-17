namespace GodotEGP.DAL.Operations;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using GodotEGP.DAL.Endpoints;
using GodotEGP.Event.Events;
using GodotEGP.Objects.Extensions;

public partial class DataOperationProcess<T>
{
	protected object _dataObject;

	public DataOperation<T> DataOperation;

	TaskCompletionSource<T> _taskCompletionSource = new TaskCompletionSource<T>();

	public DataOperationProcess(DataOperation<T> dataOperation, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
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

	public Task<T> SaveAsync()
	{
		DataOperation.Save();

		DataOperation.SubscribeOwner<DataOperationComplete>(_On_OperationCompleted, oneshot: true, isHighPriority: true);
		DataOperation.SubscribeOwner<DataOperationError>(_On_OperationError, oneshot: true, isHighPriority: true);

    	return _taskCompletionSource.Task;
	}

	public void _On_OperationCompleted(DataOperationComplete e)
	{
		_taskCompletionSource.SetResult((T) (e.RunWorkerCompletedEventArgs.Result as DataOperationResult<T>).ResultObject);
	}
	public void _On_OperationError(DataOperationError e)
	{
		_taskCompletionSource.SetException((Exception) e.RunWorkerCompletedEventArgs.Error);
	}
}

public partial class DataOperationProcessFile<T> : DataOperationProcess<T>
{
	public DataOperationProcessFile(string filePath, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new FileOperation<T>(new FileEndpoint(filePath), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessFile(FileEndpoint fileEndpoint, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new FileOperation<T>(fileEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}

public partial class DataOperationProcessHTTP<T> : DataOperationProcess<T>
{
	public DataOperationProcessHTTP(string hostname, int port = 443, string path = "/", Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new HTTPOperation<T>(new HTTPEndpoint(hostname, port, path, urlParams, requestMethod, verifySSL, timeout, headers), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessHTTP(HTTPEndpoint httpEndpoint, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new HTTPOperation<T>(httpEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}

public partial class DataOperationProcessRemoteTransfer<T> : DataOperationProcess<T>
{
	public DataOperationProcessRemoteTransfer(FileEndpoint fileEndpoint, HTTPEndpoint httpEndpoint, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new RemoteTransferOperation<T>(fileEndpoint, httpEndpoint), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}
