using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class MessageBatch
	{
		[JsonProperty(PropertyName = "batch_id")]
		public string BatchId { get; set; }

		[JsonProperty(PropertyName = "messages")]
		public List<Message> Messages { get; set; }


		public MessageBatch()
		{
			BatchId = Guid.NewGuid().ToString();
			Messages = new List<Message>();
		}

		/// <summary>
		/// Add message to batch
		/// </summary>
		/// <param name="message"></param>
		public void AddMessage(Message message)
		{
			message.Id = (uint)Messages.Count + 1;
			Messages.Add(message);
		}
	}
}
