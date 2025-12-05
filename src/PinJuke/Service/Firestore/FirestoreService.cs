using Newtonsoft.Json.Linq;
using SoftCircuits.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinJuke.Service.Firestore
{
    public class FirestoreException : Exception
    {
        public FirestoreException(string? message) : base(message)
        {
        }
    }

    public class FirestoreService
    {
        public const int ID_LENGTH = 20;

        private readonly string apiKey;
        private readonly string databaseName;

        private readonly HttpClient httpClient = new HttpClient();
        private readonly StaticValueDeserializer deserializer = new();

        public FirestoreService(string projectId, string databaseId, string apiKey)
        {
            this.apiKey = apiKey;
            this.databaseName = $"projects/{projectId}/databases/{databaseId}";
        }

        public async Task<Document?> Find(string collectionId, string id)
        {
            var uri = $"https://firestore.googleapis.com/v1/{databaseName}/documents/{collectionId}/{id}?key={Uri.EscapeDataString(apiKey)}";
            var httpResponseMessage = await httpClient.GetAsync(uri);
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            httpResponseMessage.EnsureSuccessStatusCode();
            EnsureJsonContentType(httpResponseMessage);
            var responseNode = await JsonNode.ParseAsync(await httpResponseMessage.Content.ReadAsStreamAsync());
            var document = new Document(collectionId, id);
            new GetResponseHandler(deserializer, document, responseNode).UpdateDocument();
            return document;
        }

        public async Task Insert(Document document)
        {
            await Commit(document, false, false);
        }

        public async Task Update(Document document)
        {
            await Commit(document, true, false);
        }

        public async Task Patch(Document document)
        {
            await Commit(document, true, true);
        }

        private async Task Commit(Document document, bool? exists, bool mask)
        {
            var uri = $"https://firestore.googleapis.com/v1/{databaseName}/documents:commit?key={Uri.EscapeDataString(apiKey)}";
            var requestNode = new CommitRequestBuilder(databaseName, document, exists, mask).Build();
            var requestJson = requestNode.ToJsonString();
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(uri, requestContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            EnsureJsonContentType(httpResponseMessage);
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var responseNode = await JsonNode.ParseAsync(stream);
            new CommitResponseHandler(deserializer, document, responseNode).UpdateDocument();
        }

        private void EnsureJsonContentType(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Content.Headers.ContentType?.MediaType != "application/json")
            {
                throw new HttpRequestException(@"""application/json"" response expected.");
            }
        }
    }

    /// <summary>
    /// See https://firebase.google.com/docs/firestore/reference/rest/v1/projects.databases.documents/get
    /// </summary>

    public class GetResponseHandler
    {
        private StaticValueDeserializer deserializer;
        private Document document;
        private JsonNode? responseNode;

        public GetResponseHandler(StaticValueDeserializer deserializer, Document document, JsonNode? responseNode)
        {
            this.deserializer = deserializer;
            this.document = document;
            this.responseNode = responseNode;
        }

        public void UpdateDocument()
        {
            if (responseNode is JsonObject jsonObject && jsonObject["fields"] is JsonObject fields)
            {
                document.Update(deserializer.DeserializeFields(fields));
                return;
            }
            throw new FirestoreException("Document has not been updated!");
        }
    }

    /// <summary>
    /// See https://firebase.google.com/docs/firestore/reference/rest/v1/projects.databases.documents/commit
    /// </summary>
    public class CommitRequestBuilder
    {
        private readonly string documentName;
        private readonly Document document;
        private readonly bool? exists;
        private readonly bool mask;

        public CommitRequestBuilder(string databaseName, Document document, bool? exists, bool mask)
        {
            this.document = document;
            this.exists = exists;
            this.mask = mask;

            this.documentName = $"{databaseName}/documents/{document.CollectionId}/{document.Id}";
        }

        public JsonObject Build()
        {
            return new JsonObject()
            {
                ["writes"] = BuildWrites(),
            };
        }

        private JsonArray BuildWrites()
        {
            var writesNode = new JsonArray();

            {
                var writeNode = new JsonObject()
                {
                    ["update"] = new JsonObject()
                    {
                        ["name"] = documentName,
                        ["fields"] = BuildFields(),
                    }
                };
                if (mask)
                {
                    writeNode["updateMask"] = new JsonObject()
                    {
                        ["fieldPaths"] = BuildFieldPaths(),
                    };
                }
                if (exists != null)
                {
                    writeNode["currentDocument"] = new JsonObject()
                    {
                        ["exists"] = exists,
                    };
                }
                writesNode.Add(writeNode);
            }

            if (document.TransformValuesAvailable)
            {
                var writeNode = new JsonObject()
                {
                    ["transform"] = new JsonObject()
                    {
                        ["document"] = documentName,
                        ["fieldTransforms"] = BuildFieldTransforms(),
                    }
                };
                writesNode.Add(writeNode);
            }

            return writesNode;
        }

        private JsonObject BuildFields()
        {
            return MapValue.ToFields(document.StaticValues);
        }

        private JsonArray BuildFieldPaths()
        {
            var fieldPathsNode = new JsonArray();
            foreach (var fieldName in document.FieldNames)
            {
                fieldPathsNode.Add(fieldName);
            }
            return fieldPathsNode;
        }

        private JsonArray BuildFieldTransforms()
        {
            var fieldTransformsNode = new JsonArray();
            foreach (var pair in document.TransformValues)
            {
                var fieldTransformNode = pair.Value.ToPartialFieldTransform();
                fieldTransformNode["fieldPath"] = pair.Key;
                fieldTransformsNode.Add(fieldTransformNode);
            }
            return fieldTransformsNode;
        }
    }

    public class CommitResponseHandler
    {
        private StaticValueDeserializer deserializer;
        private Document document;
        private JsonNode? responseNode;

        public CommitResponseHandler(StaticValueDeserializer deserializer, Document document, JsonNode? responseNode)
        {
            this.deserializer = deserializer;
            this.document = document;
            this.responseNode = responseNode;
        }

        public void UpdateDocument()
        {
            if (responseNode is JsonObject jsonObject && jsonObject["writeResults"] is JsonArray writeResults)
            {
                foreach (var writeResult in writeResults)
                {
                    if (writeResult!["transformResults"] is JsonArray transformResults)
                    {
                        var transformValues = document.TransformValues.Values.GetEnumerator();
                        foreach (var value in transformResults)
                        {
                            if (!transformValues.MoveNext())
                            {
                                throw new FirestoreException("End of transform values reached.");
                            }
                            var resultValue = deserializer.Deserialize((JsonObject)value!);
                            transformValues.Current.SetResultValue(resultValue);
                        }
                        break;
                    }
                }
                return;
            }
            throw new FirestoreException("Document has not been updated!");
        }
    }

    public class StaticValueDeserializer
    {
        public StaticValue Deserialize(JsonObject jsonObject)
        {
            if (jsonObject.TryGetPropertyValue("nullValue", out var nullJsonNode))
            {
                return new NullValue();
            }
            if (jsonObject.TryGetPropertyValue("stringValue", out var stringJsonNode))
            {
                return new StringValue(stringJsonNode!.GetValue<string>());
            }
            if (jsonObject.TryGetPropertyValue("integerValue", out var integerJsonNode))
            {
                return new IntegerValue(long.Parse(integerJsonNode!.GetValue<string>(), CultureInfo.InvariantCulture));
            }
            if (jsonObject.TryGetPropertyValue("booleanValue", out var booleanJsonNode))
            {
                return new BooleanValue(booleanJsonNode!.GetValue<bool>());
            }
            if (jsonObject.TryGetPropertyValue("timestampValue", out var timestampJsonNode))
            {
                return TimestampValue.Parse(timestampJsonNode!.GetValue<string>());
            }
            if (jsonObject.TryGetPropertyValue("arrayValue", out var arrayJsonNode))
            {
                StaticValue[] value = ((JsonArray)arrayJsonNode!["values"]!)
                    .Select(it => this.Deserialize((JsonObject)it!))
                    .ToArray();
                return new ArrayValue(value);
            }
            if (jsonObject.TryGetPropertyValue("mapValue", out var mapJsonNode))
            {
                var fields = (JsonObject)mapJsonNode!["fields"]!;
                return new MapValue(DeserializeFields(fields));
            }
            throw new FirestoreException("Unhandled jsonObject.");
        }

        public OrderedDictionary<string, StaticValue> DeserializeFields(JsonObject fields)
        {
            var dict = new OrderedDictionary<string, StaticValue>();
            foreach (var pair in fields)
            {
                dict[pair.Key] = Deserialize((JsonObject)pair.Value!);
            }
            return dict;
        }
    }

    public class Document : IEnumerable<KeyValuePair<string, FieldValue>>
    {
        private readonly OrderedDictionary<string, FieldValue> fields = new();

        public IList<string> FieldNames
        {
            get => fields.Keys;
        }
        public string CollectionId { get; }
        public string Id { get; }

        public FieldValue this[string name]
        {
            get
            {
                return fields[name];
            }
            set
            {
                fields[name] = value;
            }
        }

        public OrderedDictionary<string, StaticValue> StaticValues
        {
            get
            {
                var staticValues = new OrderedDictionary<string, StaticValue>();
                staticValues.AddRange(fields
                    .Where(field => field.Value is StaticValue)
                    .Select(it => KeyValuePair.Create(it.Key, (StaticValue)it.Value))
                );
                return staticValues;
            }
        }

        public OrderedDictionary<string, TransformValue> TransformValues
        {
            get
            {
                var transformValues = new OrderedDictionary<string, TransformValue>();
                transformValues.AddRange(fields
                    .Where(field => field.Value is TransformValue)
                    .Select(it => KeyValuePair.Create(it.Key, (TransformValue)it.Value))
                );
                return transformValues;
            }
        }

        public bool TransformValuesAvailable
        {
            get
            {
                return fields.Where(field => field.Value is TransformValue).Any();
            }
        }

        public Document(string collectionId, string id)
        {
            CollectionId = collectionId;
            Id = id;
        }

        public IEnumerator<KeyValuePair<string, FieldValue>> GetEnumerator()
        {
            return fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Update<T>(IEnumerable<KeyValuePair<string, T>> items) where T : FieldValue
        {
            foreach (var item in items)
            {
                this[item.Key] = item.Value;
            }
        }
    }

    public interface FieldValue
    {
    }

    public interface StaticValue : FieldValue
    {
        public JsonObject ToValue();
    }

    public interface TransformValue : FieldValue
    {
        public JsonObject ToPartialFieldTransform();
        public void SetResultValue(StaticValue resultValue);
    }

    public class NullValue : StaticValue
    {
        public object? Value
        {
            get => null;
        }

        public NullValue()
        {
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["nullValue"] = null
            };
        }
    }

    public class StringValue : StaticValue
    {
        public string Value { get; }

        public StringValue(string value)
        {
            Value = value;
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["stringValue"] = Value
            };
        }
    }

    public class IntegerValue : StaticValue
    {
        public long Value { get; }

        public IntegerValue(long value)
        {
            Value = value;
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["integerValue"] = Value.ToString(CultureInfo.InvariantCulture)
            };
        }
    }

    public class BooleanValue : StaticValue
    {
        public bool Value { get; }

        public BooleanValue(bool value)
        {
            Value = value;
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["booleanValue"] = Value
            };
        }
    }

    public class TimestampValue : StaticValue
    {
        public static TimestampValue Parse(string input)
        {
            return new TimestampValue(DateTimeOffset.Parse(input, CultureInfo.InvariantCulture));
        }

        public DateTimeOffset Value { get; }

        public TimestampValue(DateTimeOffset value)
        {
            Value = value;
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["timestampValue"] = Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture) // e.g. 2024-10-24T19:41:03.395+02:00
            };
        }
    }

    public class ArrayValue : StaticValue
    {
        public StaticValue[] Value { get; }

        public ArrayValue(StaticValue[] value)
        {
            Value = value;
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["arrayValue"] = new JsonObject()
                {
                    ["values"] = new JsonArray(Value.Select(it => it.ToValue()).ToArray())
                }
            };
        }
    }

    public class MapValue : StaticValue
    {
        public OrderedDictionary<string, StaticValue> Value { get; }

        public MapValue(OrderedDictionary<string, StaticValue> value)
        {
            Value = value;
        }

        public static JsonObject ToFields(OrderedDictionary<string, StaticValue> dict)
        {
            return new JsonObject(dict.Select(pair => KeyValuePair.Create(pair.Key, (JsonNode?)pair.Value.ToValue())));
        }

        public JsonObject ToValue()
        {
            return new JsonObject()
            {
                ["mapValue"] = new JsonObject()
                {
                    ["fields"] = ToFields(Value)
                }
            };
        }
    }

    public class ServerTimestampValue : TransformValue
    {
        public TimestampValue? ResultValue { get; set; } = null;

        public ServerTimestampValue()
        {
        }

        public JsonObject ToPartialFieldTransform()
        {
            return new JsonObject()
            {
                ["setToServerValue"] = "REQUEST_TIME"
            };
        }

        public void SetResultValue(StaticValue resultValue)
        {
            ResultValue = (TimestampValue)resultValue;
        }
    }
}
