using System.IO;
using System.Text;
using Newtonsoft.Json;
using RestSharp.Portable;

namespace StyexFleetManagement.MProfiler.Common
{
	/// <summary>
	/// Default JSON serializer for request bodies
	/// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
	/// </summary>
	public class CustomJsonSerializer : ISerializer, IDeserializer
	{
		private const string DefaultContentType = "application/json";
		private readonly Newtonsoft.Json.JsonSerializer _serializer;

		/// <summary>
		/// Default serializer
		/// </summary>
		public CustomJsonSerializer()
		{
			ContentType = DefaultContentType;
			_serializer = new Newtonsoft.Json.JsonSerializer
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include,
				DefaultValueHandling = DefaultValueHandling.Include
			};
		}

		/// <summary>
		/// Default serializer with overload for allowing custom Json.NET settings
		/// </summary>
		public CustomJsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
		{
			ContentType = DefaultContentType;
			_serializer = serializer;
		}

		/// <summary>
		/// Serialize the object as JSON
		/// </summary>
		/// <param name="obj">Object to serialize</param>
		/// <returns>JSON as String</returns>
		public byte[] Serialize(object obj)
		{
			using (var stringWriter = new StringWriter())
			{
				using (var jsonTextWriter = new JsonTextWriter(stringWriter))
				{
					jsonTextWriter.Formatting = Formatting.Indented;
					jsonTextWriter.QuoteChar = '"';

					_serializer.Serialize(jsonTextWriter, obj);

					var result = stringWriter.ToString();
                    return Encoding.UTF8.GetBytes(result);
				}
			}
		}


		/// <summary>
		/// Deserialize JSON object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		public T Deserialize<T>(IRestResponse response)
		{
			return JsonConvert.DeserializeObject<T>(response.Content);
		}


		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string DateFormat { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string RootElement { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string Namespace { get; set; }
		/// <summary>
		/// Content type for serialized content
		/// </summary>
		public string ContentType { get; set; }
	}
}
