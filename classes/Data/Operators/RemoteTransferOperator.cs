/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RemoteTransferOperator
 * @created     : Friday Feb 02, 2024 12:30:46 CST
 */

namespace GodotEGP.Data.Operator;

using System.ComponentModel;
using System.IO;
using System.Net;
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

	// download properties
	// controls whether async Start() task is allowed to run
    private volatile bool _transferAllowedToRun;

    // size to download in bytes for each request
    private long _transferChunkSize = 10000;

    // transfer bandwidth limit
    private long _transferBandwidthLimit = 999999999999;

    // IProgress instance to report progress
    private IProgress<double> _transferProgress;
    public int TransferProgress { get; set; }

    // size of the currently downloaded file
    private Lazy<long> _transferContentLength;

	// total bytes written to file
    public long TransferBytesWritten { get; private set; }
    public long TransferBytesWrittenInitial { get; private set; }
    // total bytes of file to download
    public long TransferContentLength => _transferContentLength.Value;
    // total time taken for transfer
    public TimeSpan TransferTime { get; set; }
    // transfer start time
    public DateTime TransferStartTime { get; set; }
    // transfer speed
    public long TransferSpeed { get; set; }
    // public int TransferSpeed {
    // 	get {
	// 		return (int) ((TransferBytesWritten - TransferBytesWrittenInitial) * 1000000000 / TransferTime.TotalNanoseconds);
    // 	}
    // }

	// when bytes written matches content length, it's considered finished
    public bool TransferDone => TransferContentLength == TransferBytesWritten;

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
		InitTransferState();

		try
		{
			StartTransfer().Wait();
		}
		catch (System.Exception ex)
		{
			_error = ex;

			LoggerManager.LogDebug("Error during transfer", "", "error", ex); 

			return;
		}

		// create Resource<RemoteTransfer> instance
		e.Result = new GodotEGP.Resource.Resource<Resource.RemoteTransferResult>() {
			Value = new() {
				FileEndpoint = _fileEndpoint,
				HTTPEndpoint = _httpEndpoint,
				TransferType = _operationType,
				BytesTransferred = TransferBytesWritten,
				TransferTime = TransferTime,
				TransferSpeed = TransferSpeed,
			},
			Definition = new() {
				Path = _fileEndpoint.Path,
			}
		};
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		HandleProgress(e.ProgressPercentage);
	}

	public void HandleProgress(int progressPercent)
	{
		if (progressPercent > TransferProgress)
		{
			TransferProgress = progressPercent;

			LoggerManager.LogDebug("File transfer progress", (_operationType == 0 ? "Load" : "Save"), "progress", $"progress:{progressPercent}%, speed:{TransferSpeed}, time:{TransferTime} remoteUri:{_httpEndpoint.Uri.AbsoluteUri}, fileEndpoint:{_fileEndpoint.Path}");
		}
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

	/**********************
	*  Transfer methods  *
	**********************/

    public void InitTransferState()
    {
        _transferAllowedToRun = true;
        _transferContentLength = new Lazy<long>(GetHttpEndpointContentLength);

        if (!File.Exists(_fileEndpoint.Path))
            TransferBytesWritten = 0;
        else
        {
            try
            {
                TransferBytesWritten = new FileInfo(_fileEndpoint.Path).Length;
                TransferBytesWrittenInitial = TransferBytesWritten;
            }
            catch
            {
                TransferBytesWritten = 0;
            }
        }

		if (_httpEndpoint.BandwidthLimit > 0)
		{
			_transferBandwidthLimit = _httpEndpoint.BandwidthLimit;
		}

        LoggerManager.LogDebug("Transfer content start point", "", "bytesWritten", TransferBytesWritten);
    }

    private long GetHttpEndpointContentLength()
    {
        var request = (HttpWebRequest)WebRequest.Create(_httpEndpoint.Uri.AbsoluteUri);
        request.Method = "HEAD";


        using (var response = request.GetResponse())
        {
        	LoggerManager.LogDebug("Http endpoint content length", "", "contentLength", response.ContentLength);

            return response.ContentLength;
        }
    }

    private async Task StartTransfer(long range)
    {
        if (!_transferAllowedToRun)
        {
        	LoggerManager.LogDebug("Not allowed to run!");

            throw new InvalidOperationException();
        }

        if(TransferDone)
        {
            //file has been found in folder destination and is already fully downloaded 
        	LoggerManager.LogDebug("Transfer file exists", "", "fileEndpoint", _fileEndpoint);

            return;
        }

        LoggerManager.LogDebug("Starting transfer", "", "httpEndpointUri", _httpEndpoint.Uri.AbsoluteUri);
        LoggerManager.LogDebug("", "", "fileEndpoint", _fileEndpoint);

		TransferStartTime = DateTime.Now;

        var request = (HttpWebRequest)WebRequest.Create(_httpEndpoint.Uri.AbsoluteUri);
        request.Method = "GET";
        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        request.AddRange(range);

        using (var response = await request.GetResponseAsync())
        {
            using (var responseStream = response.GetResponseStream())
            {
                using (var fs = new FileStream(_fileEndpoint.Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                	long bytesReadLastSec = 0;
                	int readIterationsLastSec = 0;
                	int delayMs = 0;
                	DateTime prevBytesReadTime = DateTime.Now;

                	if (_transferBandwidthLimit < _transferChunkSize)
                	{
                		LoggerManager.LogDebug("Limiting chunk size to bandwidth limit", "", "limit", _transferBandwidthLimit);

                		_transferChunkSize = _transferBandwidthLimit;
                	}

                    while (_transferAllowedToRun)
                    {
                    	if (bytesReadLastSec < _transferBandwidthLimit)
                    	{
                        	var buffer = new byte[_transferChunkSize];
                        	var bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                        	if (bytesRead == 0) break;

                        	await fs.WriteAsync(buffer, 0, bytesRead);

                        	readIterationsLastSec++;

                        	bytesReadLastSec += _transferChunkSize;

                        	TransferBytesWritten += bytesRead;

                        	delayMs = (DateTime.Now - prevBytesReadTime).Milliseconds / Math.Max(1, readIterationsLastSec);
                    	}

						// adjust download speed
                        if ((DateTime.Now - prevBytesReadTime).Seconds >= 1)
                        {
                        	LoggerManager.LogDebug("Bytes read last sec", "", "bytes", bytesReadLastSec);
                        	LoggerManager.LogDebug("Read iterations last sec", "", "reads", readIterationsLastSec);
                        	LoggerManager.LogDebug("Target bytes per sec", "", "target", _transferBandwidthLimit);

							double bytesReadTargetPercent = ((double) bytesReadLastSec) / ((double) _transferBandwidthLimit);

							LoggerManager.LogDebug("Percent on target", "", "percent", bytesReadTargetPercent);

							LoggerManager.LogDebug("Updating delay time ms", "", "delayMs", delayMs);


                        	bytesReadLastSec = 0;
                        	readIterationsLastSec = 0;
                        	prevBytesReadTime = DateTime.Now;
                        }

						TransferSpeed = bytesReadLastSec;

						await Task.Delay(delayMs);

						TransferTime = DateTime.Now - TransferStartTime;

						var progress = (double)TransferBytesWritten / TransferContentLength;
                        _transferProgress?.Report(progress * 100);
                        ReportProgress((int) (progress * 100));
                    }

                    await fs.FlushAsync();
                }
            }
        }

        LoggerManager.LogDebug("Transfer process finished");
    }

    public Task StartTransfer()
    {
        _transferAllowedToRun = true;
        return StartTransfer(TransferBytesWritten);
    }

    public void Pause()
    {
        _transferAllowedToRun = false;
    }
}
