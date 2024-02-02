/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RemoteTransferOperator
 * @created     : Friday Feb 02, 2024 12:30:46 CST
 */

namespace GodotEGP.Data.Operator;

using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

using GodotEGP.Logging;
using GodotEGP.Data.Endpoint;

// operates on file based objects 
public partial class RemoteTransferOperator : Operator, IOperator
{
	private FileEndpoint _fileEndpoint;
	private HTTPEndpoint _httpEndpoint;
	private int _operationType;

	private object _dataObject;

	public void Load(object dataObj = null)
	{
		LoggerManager.LogDebug($"Upload to endpoint", "", "endpoint", _httpEndpoint);
		LoggerManager.LogDebug($"Upload from endpoint", "", "endpoint", _fileEndpoint);

		_operationType = 0;

		Run();
	}

	public void Save(object dataObj = null)
	{
		LoggerManager.LogDebug($"Download from endpoint", "", "endpoint", _httpEndpoint);
		LoggerManager.LogDebug($"Download to endpoint", "", "endpoint", _fileEndpoint);

		_dataObject = dataObj;

		_operationType = 1;

		Run();
	}

	public void SetDataEndpoint(IEndpoint dataEndpoint) {
		_fileEndpoint = (FileEndpoint) dataEndpoint;
	}
	public void SetHttpEndpoint(IEndpoint dataEndpoint) {
		_httpEndpoint = (HTTPEndpoint) dataEndpoint;
	}

	public FileEndpoint GetDataEndpoint()
	{
		return _fileEndpoint;
	}

	// background job methods
	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		switch (_operationType)
		{
			case 0:
				LoadOperationDoWork(sender, e);
				break;
			case 1:
				SaveOperationDoWork(sender, e);
				break;
			default:
				break;
		}
	}

	public void LoadOperationDoWork(object sender, DoWorkEventArgs e)
	{
		throw new NotImplementedException("Load/Upload operation is not implemented");
	}

	public void SaveOperationDoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Save operation starting");

		// ensure that the base directory of the download path exists
		EnsureDirectoryExists(_fileEndpoint.Path);

		// download the file using a resumable stream instance
		// TODO
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Progress changed!", "", "progress", e.ProgressPercentage);
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		switch (_operationType)
		{
			case 0:
				LoadOperationRunWorkerCompleted(sender, e);
				break;
			case 1:
				SaveOperationRunWorkerCompleted(sender, e);
				break;
			default:
				break;
		}
	}

	public void LoadOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		// LoggerManager.LogDebug("Load operation completed", "", "result", e.Result);
		LoggerManager.LogDebug("Load operation completed");
	}
	public void SaveOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Save operation completed", "", "result", e.Result);
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operator failed");
	}

	private static void EnsureDirectoryExists(string filePath) 
	{ 
  		FileInfo fi = new FileInfo(filePath);
  		if (!fi.Directory.Exists) 
  		{ 
    		System.IO.Directory.CreateDirectory(fi.DirectoryName); 
  		} 
	}
}

