/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : OpenAI
 * @created     : Sunday Jan 14, 2024 18:36:16 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public partial class OpenAI
{
	public OpenAIConfig Config { get; set; }
	public ErrorResult Error  { get; set; }

	public OpenAI(OpenAIConfig config)
	{
		Config = config;
	}

	public async Task<string> MakeRequestPost(string endpoint = "", object requestObj = null, bool useSse = false)
	{
		LoggerManager.LogDebug("Making request", "", "requestObj", requestObj);

		var httpClient = new HttpClient();
		httpClient.Timeout = TimeSpan.FromSeconds(600);

		var jsonContent = JsonConvert.SerializeObject(requestObj, 
        				new JsonSerializerSettings
					{
    					ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() },
					});

		// serialise object to snake case json
		var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
		httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.APIKey);

		LoggerManager.LogDebug("Making request body", "", "requestJson", jsonContent);

		// send the request to the server and watch for server sent events if
		// stream: true
		string responseString = "";

		using (var request = new HttpRequestMessage(HttpMethod.Post, Config.Host+endpoint){ 
				Content = content
				})
		using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))

		if (useSse)
		{
			using (var theStream = await response.Content.ReadAsStreamAsync())
			using (var theStreamReader = new StreamReader(theStream))
			{
    			string sseLine = null;
    			string sseLines = "";

    			while ((sseLine = await theStreamReader.ReadLineAsync()) != null)
    			{
    				// hackily parse server sent events
    				if (sseLine.StartsWith("data: "))
    				{
    					LoggerManager.LogDebug("SSE event received", "", "sseEvent", sseLine);

    					this.Emit<OpenAIServerSentEvent>(e => {
    							e.Event = sseLine.Replace("data: ", "");

    							if (e.Event != "[DONE]")
    							{
    								e.Chunk = JsonConvert.DeserializeObject<ChatCompletionChunkResult>(sseLine.Replace("data: ", ""), new JsonSerializerSettings {
    									ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }}
									);

    							}
    						});
    				}
    				else if (sseLine.Length > 0) {
						LoggerManager.LogDebug("Line received", "", "line", sseLine);

						sseLines += sseLine+"\n";
    				}
    			}

    			// check for SSE lines
    			if (sseLines.Length > 0)
    			{
					var errorResult = await GetResultObject<ErrorResult>(sseLines);

					if (errorResult.Error != null)
					{
						Error = errorResult;

						LoggerManager.LogDebug("Request error response", "", "error", Error);

						this.Emit<OpenAIError>(e => e.Error = Error);

						return "";
					}
    			}
			};

			httpClient.CancelPendingRequests();
			request.Dispose();
			response.Dispose();
			httpClient.Dispose();

			this.Emit<OpenAIStreamingFinished>();
		}
		else
		{
			var contents = await response.Content.ReadAsStringAsync();
			request.Dispose();
			response.Dispose();
			httpClient.Dispose();

			var errorResult = await GetResultObject<ErrorResult>(contents);

			if (errorResult.Error != null)
			{
				Error = errorResult;

				LoggerManager.LogDebug("Request error response", "", "error", Error);

				this.Emit<OpenAIError>(e => e.Error = Error);

				return "";
			}

			responseString = contents;

		}
		return responseString;
	}

	public async Task<string> MakeRequestGet(string endpoint = "")
	{
		string contents = "";
		using (var client = new HttpClient(new HttpClientHandler {  }))
        {
            client.BaseAddress = new Uri(Config.Host);
            HttpResponseMessage response = client.GetAsync(endpoint).Result;
            contents = response.Content.ReadAsStringAsync().Result;
        }

		var errorResult = await GetResultObject<ErrorResult>(contents);

		if (errorResult.Error != null)
		{
			Error = errorResult;

			LoggerManager.LogDebug("Request error response", "", "error", Error);

			this.Emit<OpenAIError>(e => e.Error = Error);

			return "";
		}

		return contents;
	}

	public async Task<T> GetResultObject<T>(string jsonObj) where T : BaseResult, new()
	{
		T resultObj = null;

		try
		{
    		resultObj = JsonConvert.DeserializeObject<T>(jsonObj, new JsonSerializerSettings {
    			ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }}
			);
		}
		catch (System.Exception e)
		{
			LoggerManager.LogError("Deserialising result object failed", "", "jsonObj", jsonObj);

			if (resultObj is ErrorResult err)
			{
				err = new();
				err.Error = new() {
					Type = "internal_error",
					Code = "deserialisation error",
					Message = jsonObj,
				};
			}
		}

		return resultObj;
	}

	// /v1/embeddings
	public async Task<EmbeddingsResult> Embeddings(EmbeddingsRequest request)
	{
		var r = await GetResultObject<EmbeddingsResult>(await MakeRequestPost("/v1/embeddings", request, false));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}

	// /v1/completions
	public async Task<CompletionResult> Completions(CompletionRequest request)
	{
		var r = await GetResultObject<CompletionResult>(await MakeRequestPost("/v1/completions", request, request.Stream));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}
	
	// /v1/chat/completions
	public async Task<ChatCompletionResult> ChatCompletions(ChatCompletionRequest request)
	{
		var r = await GetResultObject<ChatCompletionResult>(await MakeRequestPost("/v1/chat/completions", request, request.Stream));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}

	// /v1/models
	public async Task<ModelsResult> Models()
	{
		var r = await GetResultObject<ModelsResult>(await MakeRequestGet("/v1/models"));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}

	// /v1/models/{model}
	public async Task<ModelResult> Models(string model)
	{
		var r = await GetResultObject<ModelResult>(await MakeRequestGet($"/v1/models/{model}"));

		this.Emit<OpenAIResult>(e => e.Result = r);

		return r;
	}
}

public partial class OpenAIEvent : Event {}
public partial class OpenAIError : OpenAIEvent { 
	public ErrorResult Error { get; set; }
}
public partial class OpenAIResult : OpenAIEvent { 
	public BaseResult Result { get; set; }
}
public partial class OpenAIServerSentEvent : OpenAIEvent {
	public string? Event { get; set; }
	public ChatCompletionChunkResult? Chunk { get; set; }
}
public partial class OpenAIStreamingFinished : OpenAIEvent { 
}
