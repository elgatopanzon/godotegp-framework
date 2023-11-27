namespace GodotEGP.Data.Operator;

using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

using GodotEGP.Logging;
using GodotEGP.Data.Endpoint;

// operates on file based objects 
public partial class FileOperator : Operator, IOperator
{
	private FileEndpoint _fileEndpoint;
	private int _operationType;

	private object _dataObject;

	public void Load()
	{
		LoggerManager.LogDebug($"Load from endpoint", "", "endpoint", _fileEndpoint);

		_operationType = 0;

		Run();
	}

	public void Save(object dataObj)
	{
		LoggerManager.LogDebug($"Save to endpoint", "", "endpoint", _fileEndpoint);
		// LoggerManager.LogDebug($"", "", "dataObj", dataObj);

		_dataObject = dataObj;

		_operationType = 1;

		Run();
	}

	public void SetDataEndpoint(IEndpoint dataEndpoint) {
		_fileEndpoint = (FileEndpoint) dataEndpoint;
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
		LoggerManager.LogDebug("Load operation starting");
    	using (StreamReader reader = new StreamReader(_fileEndpoint.Path))
    	{
    		e.Result = reader.ReadToEnd();
    		ReportProgress(100);
    	}
	}

	public void SaveOperationDoWork(object sender, DoWorkEventArgs e)
	{
		// LoggerManager.LogDebug("Save operation starting", "", "object", _dataObject);
		LoggerManager.LogDebug("Save operation starting");

		EnsureDirectoryExists(_fileEndpoint.Path);

    	using (StreamWriter writer = new StreamWriter(_fileEndpoint.Path))
    	{
			// for now, serialise the object as json
			var jsonString = JsonConvert.SerializeObject(
        			_dataObject, Formatting.Indented);

    		writer.WriteLine(jsonString);

    		e.Result = true;
    		ReportProgress(100);
    	}
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

