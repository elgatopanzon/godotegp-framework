/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChatCompletionResultMessage
 * @created     : Sunday Jan 21, 2024 20:58:17 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class ChatCompletionResultMessage
{
	public object Content { get; set; }
	public string Role { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public List<ChatCompletionResultToolCall> ToolCalls { get; set; }
	
	public ChatCompletionResultMessage()
	{
	}

	public string GetContent()
	{
		if (Content is Newtonsoft.Json.Linq.JArray)
		{
			LoggerManager.LogDebug("Contents object", "", "contents", GetContents());

			return String.Join(" ", GetContents().Where(x => x.Type == "text").Select(x => x.Text).ToArray<string>());
		}
		else if (Content is List<ChatCompletionResultMessageContent> dto)
		{
			return String.Join(" ", GetContents().Where(x => x.Type == "text").Select(x => x.Text).ToArray<string>());
		}
		else if (Content is string)
		{
			return (string) Content;
		}
		else
		{
			return (string) JsonConvert.SerializeObject(Content);
		}
	}
	public List<ChatCompletionResultMessageContent> GetContents()
	{
		List<ChatCompletionResultMessageContent> contentDtos = new();

		if (Content is Newtonsoft.Json.Linq.JArray c)
		{

			foreach (Newtonsoft.Json.Linq.JToken content in c)
			{
				IDictionary<string,object> dict = content.ToObject<Dictionary<string, object>>();

				ChatCompletionResultMessageContent contentDto = new();

				if (dict.TryGetValue("type", out var type))
				{
					contentDto.Type = (string) type;
				}
				if (dict.TryGetValue("text", out var text))
				{
					contentDto.Text = (string) text;
				}
				if (dict.TryGetValue("image_url", out var imageUrl))
				{
					var values = JObject.FromObject(imageUrl).ToObject<Dictionary<string, object>>();

					if (values.TryGetValue("url", out var url))
					{
						contentDto.ImageUrl = (string) url;
					}
				}

				contentDtos.Add(contentDto);
			}
		}
		else if (Content is List<ChatCompletionResultMessageContent> dto)
		{
			contentDtos = dto;
		}

		LoggerManager.LogDebug("Content dtos", "", "contentDtos", contentDtos);

		return contentDtos;
	}
}

public partial class ChatCompletionResultMessageContent
{
	public string Type { get; set; } = "text";
	public string Text { get; set; } = "";
	public string ImageUrl { get; set; } = "";
}
