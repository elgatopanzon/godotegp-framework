namespace GodotEGP.Config;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using GodotEGP.Logging;
using GodotEGP.Threading;
using GodotEGP.DAL.Endpoints;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;

public partial class ConfigLoader : BackgroundJob
{
	private Queue<Dictionary<string, object>> _loadQueue = new Queue<Dictionary<string, object>>();

	private ConfigObject _currentlyLoadingObj;
	private double _queueSize = 0;

	private double _queueSizeCurrent { get {
		return _loadQueue.Count;
	} }

	private List<ConfigObject> _configObjects = new List<ConfigObject>();

	public ConfigLoader(Queue<Dictionary<string, object>> loadQueue)
	{
		_loadQueue = loadQueue;

		Run();
	}

	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug($"{_loadQueue.Count} config files queued for loading");

		_queueSize = _loadQueue.Count;

		try
		{
			// keep running until the queue is empty
			while (_queueSizeCurrent > 0)
			{
				if (_loadQueue.TryPeek(out var queuedItem))
				{
					// if currently loading object is null, then we can queue a new
					// load
					if (_currentlyLoadingObj == null)
					{
						LoggerManager.LogDebug("Loading config item", "", "config", queuedItem);

						// create config object instance to load into
						var obj = ConfigObject.Create(queuedItem["configType"].ToString());

						if (queuedItem.TryGetValue("name", out object name))
						{
							obj.Name = name.ToString();
						}
						else
						{
							// use a generic name when the dictionary doesn't
							// include a name
							obj.Name = queuedItem["configType"].ToString()+(_queueSize - _queueSizeCurrent).ToString();
						}

						// set data endpoint to current instance's file path
						obj.DataEndpoint = new FileEndpoint(queuedItem["path"].ToString());

						var tcs = new TaskCompletionSource();

						obj.SubscribeOwner<DataOperationComplete>((ee) => {
							tcs.SetResult();
							}, isHighPriority:true, oneshot:true);
						obj.SubscribeOwner<DataOperationError>((ee) => {
							tcs.SetResult();
							// throwing this should be a configurable engine
							// setting, since it will stop the entire loading
							// process and cause the loader to fail just because
							// of one config file containing invalid data
							//
							// what will happen is the config object will
							// contain the default property value instead
							// throw ee.RunWorkerCompletedEventArgs.Error;
							}, isHighPriority:true, oneshot:true);

						obj.Load();

						_currentlyLoadingObj = obj;

						// wait for the result
						tcs.Task.Wait();
					}
					// while loading, just do nothing?
					else if (_currentlyLoadingObj.Loading)
					{
					}
					// if it's not loading, then assume the process ended and queue
					// the next one
					else if (!_currentlyLoadingObj.Loading)
					{
						LoggerManager.LogDebug("Loading config item process finished", "", "config", queuedItem);

						// add loaded object to list
						_configObjects.Add(_currentlyLoadingObj);

						e.Result = _configObjects;

						// reset currently loading object
						_currentlyLoadingObj = null;
						_loadQueue.Dequeue();


						// report progress of the load
						double progress = ((_queueSize - _queueSizeCurrent) / _queueSize) * 100;
						ReportProgress(Convert.ToInt32(progress));

						// this just allows for the progress reports to be sent
						// before the loop continues
						if (progress != 100)
						{
							System.Threading.Thread.Sleep(50);
						}
					}
				}
			}
		}
		catch (System.Exception err)
		{
			_error = err;
		}
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs progress", "", "progress", e.ProgressPercentage);

		this.Emit<ConfigManagerLoaderProgress>((ee) => ee.SetProgressChangesEventArgs(e).SetProgressChangesEventArgs(e));
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		// LoggerManager.LogDebug("Loading configs completed", "", "result", e.Result);
		LoggerManager.LogDebug("Loading configs completed");

		this.Emit<ConfigManagerLoaderCompleted>((ee) => ee.SetConfigObjects(_configObjects).SetRunWorkerCompletedEventArgs(e));
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs error");

		this.Emit<ConfigManagerLoaderError>((ee) => ee.SetConfigObjects(_configObjects).SetRunWorkerCompletedEventArgs(e));
	}
}
