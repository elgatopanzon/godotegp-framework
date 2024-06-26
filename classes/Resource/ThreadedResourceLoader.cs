/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Loader
 * @created     : Saturday Nov 11, 2023 18:00:26 CST
 */

namespace GodotEGP.Resource;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Threading;
using GodotEGP.DAL.Endpoints;
using GodotEGP.Resource.Resources;

using System.Security.Cryptography;
using System.IO;

public partial class ThreadedResourceLoader : BackgroundJob
{
	private Queue<LoaderQueueItem> _loadQueue = new Queue<LoaderQueueItem>();

	private double _queueSize = 0;

	private double _queueSizeCurrent { get {
		return _loadQueue.Count;
	} }

	private List<LoaderQueueItem> _resourceObjects = new List<LoaderQueueItem>();

	public ThreadedResourceLoader(Queue<LoaderQueueItem> loadQueue)
	{
		_loadQueue = loadQueue;
		_queueSize = loadQueue.Count;

		Run();
	}

	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		var item =_loadQueue.Dequeue();

		LoggerManager.LogDebug("Loading resource", "", "resource", $"{item.Category}:{item.Id}");

		var resourceObject = (dynamic) ResourceObjectBase.Create(item.ResourceDefinition.ClassType);

		Resource r = (Resource) Activator.CreateInstance(item.ResourceDefinition.ClassType);

		// load the resource using resource loader if it's a res:// path
		if (item.ResourceDefinition.IsResourcePath())
		{
			if (ResourceLoader.Exists(item.ResourceDefinition.Path))
			{
				r = GD.Load(item.ResourceDefinition.Path);
			}
			else
			{
				// if the resource class type is GameScript, load the content of the
				// game script file while applying project base res:// directory and
				// loading content into Config
				// TODO: make this work better
				if (item.ResourceDefinition.ClassType.Equals(typeof(GameScript)))
				{
    				using (StreamReader reader = new StreamReader(ProjectSettings.GlobalizePath(item.ResourceDefinition.Path)))
    				{
    					var res = reader.ReadToEnd();

						if (r is GameScript gs)
						{
							gs.ScriptContent = res;
						}
    				}
				}

			}
			
			// if it's still null, then it's not found
			if (r == null)
			{
				throw new ResourceNotFoundException($"Resource not found {item.ResourceDefinition.Path}");
			}

			LoggerManager.LogDebug("Loaded resource from path", "", "resource", r.GetType().Name);
		}

		resourceObject.Value = (dynamic) r;

		if (item.ResourceDefinition.Config != null)
		{
			Newtonsoft.Json.JsonConvert.PopulateObject((string) JsonConvert.SerializeObject(item.ResourceDefinition.Config, Formatting.Indented),
					resourceObject.Value,
				new JsonSerializerSettings
    			{
        			Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        			{
            			args.ErrorContext.Handled = true;
        			},
        			ObjectCreationHandling = ObjectCreationHandling.Replace
    			}
			);
		}

		resourceObject.Id = item.Id;
		resourceObject.Category = item.Category;
		resourceObject.Definition = item.ResourceDefinition;

		string fileHash = "";
		using (var md5 = MD5.Create())
		{
    		if (File.Exists(item.ResourceDefinition.Path))
    		{
    			using (var stream = File.OpenRead(item.ResourceDefinition.Path))
    			{
        			fileHash = String.Join("", md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(item.ResourceDefinition.Path)));
    			}
    		}
		}

		item.ResourceDefinition.FileHash = fileHash;

		item.ResourceObject = resourceObject;
		_resourceObjects.Add(item);

		// report progress
		double progress = ((_queueSize - _queueSizeCurrent) / _queueSize) * 100;
		ReportProgress(Convert.ToInt32(progress));
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs progress", "", "progress", e.ProgressPercentage);

		this.Emit<ResourceLoaderProgress>((ee) => ee.SetProgressChangesEventArgs(e).SetProgressChangesEventArgs(e));
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Loading resources completed", "", "result", e.Result);

		if (_queueSizeCurrent > 0)
		{
			Run();
		}
		else
		{
			LoggerManager.LogDebug("Loading resource queue completed");

			this.Emit<ResourceLoaderCompleted>((ee) => ee.SetResources(_resourceObjects).SetRunWorkerCompletedEventArgs(e));
		}
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Loading resources error");

		this.Emit<ResourceLoaderError>((ee) => ee.SetResources(_resourceObjects).SetRunWorkerCompletedEventArgs(e));
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class ResourceNotFoundException : Exception
	{
		public ResourceNotFoundException() {}
		public ResourceNotFoundException(string message) : base(message) {}
		public ResourceNotFoundException(string message, Exception inner) : base(message, inner) {}
	}
}

public partial class LoaderQueueItem
{
	private string _category;
	public string Category
	{
		get { return _category; }
		set { _category = value; }
	}

	private string _id;
	public string Id
	{
		get { return _id; }
		set { _id = value; }
	}

	private ResourceDefinition _resourceDefinition;
	public ResourceDefinition ResourceDefinition
	{
		get { return _resourceDefinition; }
		set { _resourceDefinition = value; }
	}

	private ResourceObjectBase _resourceObject;
	public ResourceObjectBase ResourceObject
	{
		get { return _resourceObject; }
		set { _resourceObject = value; }
	}

	public LoaderQueueItem(string category, string id, ResourceDefinition resourceDefinition)
	{
		_category = category;
		_id = id;
		_resourceDefinition = resourceDefinition;
	}
}
