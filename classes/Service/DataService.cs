namespace GodotEGP.Service;

using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;

using GodotEGP.Event.Events;
using GodotEGP.Data.Operation;
using GodotEGP.Data.Endpoint;

public partial class DataService : Service
{
	// load/save to/from file endpoint
	public Operation LoadFromFile<T>(string filePath, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		var dopf = DataOperationFromEndpoint<T>(new FileEndpoint(filePath), null, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);
		dopf.Load();

		return dopf.DataOperation;
	}

	public Operation SaveToFile<T>(string filePath, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		var dopf = DataOperationFromEndpoint<T>(new FileEndpoint(filePath), null, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);
		dopf.Save();

		return dopf.DataOperation;
	}

	public Operation HTTPRequest<T>(string hostname, int port = 443, string path = "/", Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		var doph = DataOperationFromEndpoint<T>(new HTTPEndpoint(hostname, port, path, urlParams, requestMethod, verifySSL, timeout, headers), dataObject, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);
		doph.Save();

		return doph.DataOperation;
	}

	public OperationProcess<T> DataOperationFromEndpoint<T>(IEndpoint dataEndpointObject, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		if (dataEndpointObject is FileEndpoint def)
		{
			DataOperationProcessFile<T> dopf = new DataOperationProcessFile<T>(def, dataObject, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);

			return dopf;
		}
		else if (dataEndpointObject is HTTPEndpoint deh)
		{
			DataOperationProcessHTTP<T> dopf = new DataOperationProcessHTTP<T>(deh, dataObject, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);

			return dopf;
		}
		else
		{
			throw new NotImplementedException($"{dataEndpointObject.GetType().Name} type is not implemented!");
		}
	}
}
