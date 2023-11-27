namespace GodotEGP.Data.Operator;

using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

using GodotEGP.Data.Endpoint;
using GodotEGP.Logging;

// operates on file based objects 
public partial class HTTPOperator : Operator, IOperator
{
	private HTTPEndpoint _httpEndpoint;
	private int _operationType;

	private object _dataObject;

	public void Load()
	{
		LoggerManager.LogDebug($"Load from endpoint", "", "endpoint", _httpEndpoint);

		_operationType = 0;

		Run();
	}

	public void Save(object dataObj)
	{
		LoggerManager.LogDebug($"Save to endpoint", "", "endpoint", _httpEndpoint);
		LoggerManager.LogDebug($"", "", "dataObj", dataObj);

		_dataObject = dataObj;

		_operationType = 1;

		Run();
	}

	public void SetDataEndpoint(IEndpoint dataEndpoint) {
		_httpEndpoint = (HTTPEndpoint) dataEndpoint;
	}

	public HTTPEndpoint GetDataEndpoint()
	{
		return _httpEndpoint;
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
    	// using (StreamReader reader = new StreamReader(_httpEndpoint.Path))
    	// {
    	// 	e.Result = reader.ReadToEnd();
    	// 	ReportProgress(100);
    	// }
	}

	public void SaveOperationDoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Save operation starting", "", "object", _dataObject);

    	// using (StreamWriter writer = new StreamWriter(_httpEndpoint.Path))
    	// {
		// 	// for now, serialise the object as json
		// 	var jsonString = JsonConvert.SerializeObject(
        // 	_dataObject, Formatting.Indented);
        //
    	// 	writer.WriteLine(jsonString);
        //
    	// 	e.Result = true;
    	// 	ReportProgress(100);
    	// }
		var client = new System.Net.Http.HttpClient();
		client.Timeout = System.TimeSpan.FromSeconds(_httpEndpoint.Timeout);

        try
        {
    		// create web request with endpoints data
    		var webRequest = new HttpRequestMessage();

    		webRequest.RequestUri = _httpEndpoint.Uri;
    		webRequest.Method = _httpEndpoint.RequestMethod;

			// if data object is a HttpContent object, then set it
    		if (_dataObject is HttpContent content)
    		{
    			webRequest.Content = content;
    		}
    		else if (_dataObject != null)
    		{
    			webRequest.Content = new StringContent(JsonConvert.SerializeObject(_dataObject), Encoding.UTF8, "application/json");
    		}

    		var response = client.Send(webRequest);
    		response.EnsureSuccessStatusCode();

    		using var reader = new StreamReader(response.Content.ReadAsStream());

    		var res = reader.ReadToEnd();

    		e.Result = res;
        }
        catch (System.Exception ex)
        {
			throw;
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
		LoggerManager.LogDebug("Load operation completed", "", "result", e.Result);
	}
	public void SaveOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Save operation completed", "", "result", e.Result);
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operator failed");
	}
}


