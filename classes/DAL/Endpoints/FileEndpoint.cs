namespace GodotEGP.DAL.Endpoints;

using GodotEGP.Logging;
using Godot;

// File object holding information about the provided filename and path
public partial class FileEndpoint : IDataEndpoint
{
	private string _path;
	private string _extension;
	private string _mimetype;

	public string Path
	{
		get { return _path; }
		set { _path = value; }
	}

	public string Extension
	{
		get { return _extension; }
		set { _extension = value; }
	}

	public string Mimetype
	{
		get { return _mimetype; }
		set { _mimetype = value; }
	}

	public FileEndpoint(string filePath)
	{
		// first globalize the path
		filePath = ProjectSettings.GlobalizePath(filePath);

        // get platform safe path from a provided unix path (because we use
        // that, because godot uses that even for windows)
        if (filePath.StartsWith("/"))
        {
        	_path = "/"+System.IO.Path.Combine(filePath.Split("/"));
        }
        else
        {
        	_path = System.IO.Path.GetFullPath(System.IO.Path.Combine(filePath.Split("/")));
        }
        _extension = System.IO.Path.GetExtension(_path);
        _mimetype = MimeType.GetMimeType(_extension);
	}
}

