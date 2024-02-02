namespace GodotEGP.Data.Operator;

using GodotEGP.Data.Endpoint;

// interface for classes which perform direct data operations using
// IDataEndpointObject instances
public partial interface IOperator
{
	void SetDataEndpoint(IEndpoint dataEndpoint);
	void Load(object dataObj = null);
	void Save(object dataObj = null);
}

