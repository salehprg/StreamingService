using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

public class SingleValueArrayConverter<T> : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            object retVal = new Object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            } 
            else if (reader.TokenType == JsonToken.StartArray) 
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }
        catch
        {
            return null;
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}