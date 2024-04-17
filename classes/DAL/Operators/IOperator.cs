namespace GodotEGP.DAL.Operator;

using GodotEGP.DAL.Endpoints;

// interface for classes which perform direct data operations using
// IDataEndpointObject instances
public partial interface IOperator
{
	void SetDataEndpoint(IDataEndpoint dataEndpoint);
	void Load(object dataObj = null);
	void Save(object dataObj = null);
}

