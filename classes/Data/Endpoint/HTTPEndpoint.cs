namespace GodotEGP.Data.Endpoint;

using System.Collections.Generic;
using System.Net.Http;

using GodotEGP.Logging;

// File object holding information about the provided filename and path
public partial class HTTPEndpoint : IEndpoint
{
	private string _hostname;
	public string Hostname
	{
		get { return _hostname; }
		set { _hostname = value; }
	}

	private int _port;
	public int Port
	{
		get { return _port; }
		set { _port = value; }
	}

	private string _path;
	public string Path
	{
		get { return _path; }
		set { _path = value; }
	}

	private HttpMethod _requestMethod;
	public HttpMethod RequestMethod
	{
		get { return _requestMethod; }
		set { _requestMethod = value; }
	}

	private bool _verifySsl;
	public bool VerifySsl
	{
		get { return _verifySsl; }
		set { _verifySsl = value; }
	}

	private int _timeout;
	public int Timeout
	{
		get { return _timeout; }
		set { _timeout = value; }
	}

	private Dictionary<string, string> _headers;
	public Dictionary<string, string> Headers
	{
		get { return _headers; }
		set { _headers = value; }
	}

	private System.Uri _uri;
	public System.Uri Uri
	{
		get { return _uri; }
		set { _uri = value; }
	}

	private Dictionary<string, object> _params;
	public Dictionary<string, object> Params
	{
		get { return _params; }
		set { _params = value; }
	}

	private long _bandwidthLimit;
	public long BandwidthLimit
	{
		get { return _bandwidthLimit; }
		set { _bandwidthLimit = value; }
	}

	public HTTPEndpoint(string hostname, int port = 0, string path = "/", Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null)
	{
		SetUriFromParams(hostname, port, path, (port == 443), urlParams);
		SetPropertiesFromUri();

		_requestMethod = requestMethod;
		_verifySsl = verifySSL;
		_timeout = timeout;
		_headers = headers;
		_params = urlParams;
	}

	public HTTPEndpoint(string uri, Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null)
	{
		_uri = new System.Uri(uri);

		_requestMethod = requestMethod;
		_verifySsl = verifySSL;
		_timeout = timeout;
		_headers = headers;

		// TODO: create _params dictionary from uri string
	}

	public HTTPEndpoint(Uri uri, Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null)
	{
		_uri = uri;

		_requestMethod = requestMethod;
		_verifySsl = verifySSL;
		_timeout = timeout;
		_headers = headers;
	}

	public void SetUriFromParams(string host, int port, string path, bool useSsl = true, Dictionary<string,object> urlParams = null)
	{
		_uri = new System.Uri($"{(useSsl ? "https" : "http")}://{host}:{port}{path}{QueryString(urlParams)}");
	}

	public void SetPropertiesFromUri()
	{
		_hostname = _uri.Host;
		_port = _uri.Port;
		_path = _uri.PathAndQuery;
	}

	public string QueryString(IDictionary<string, object> dict)
	{
    	var list = new List<string>();
    	foreach(var item in dict)
    	{
        	list.Add(item.Key + "=" + item.Value);
    	}
    	return $"{(list.Count > 0 ? "?" : "")}"+string.Join("&", list);
	}
}
