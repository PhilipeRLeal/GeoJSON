﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BAMCIS.GeoJSON.Serde
{
    /// <summary>
    /// Converts Geometry objects
    /// </summary>
    public class GeometryConverter : JsonConverter
    {

        #region Public Properties

        /// <summary>
        /// Make sure this converter is used to deserialize
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Use the default serializer
        /// </summary>
        public override bool CanWrite => false;

        #endregion

        /// <summary>
        /// Object of GeoJson type can be converted with this converter
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Geometry);
        }

        /// <summary>
        /// Reads the json and converts to appropriate Geometry class using the 'type' field as an indicator
        /// to which object it should be deserialized back to
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Geometry can be null in a feature
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject token = JObject.Load(reader);

            if (!token.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out JToken TypeToken))
            {
                throw new JsonReaderException("Invalid geojson geometry object, does not have 'type' field.");
            }

            Type actualType = Geometry.GetType(TypeToken.ToObject<GeoJsonType>(serializer));

            if (existingValue == null || existingValue.GetType() != actualType)
            {
                return token.ToObject(actualType, serializer);
            }
            else
            {
                using (JsonReader DerivedTypeReader = token.CreateReader())
                {
                    serializer.Populate(DerivedTypeReader, existingValue);
                }

                return existingValue;
            }
        }

        /// <summary>
        /// Use the default serializer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
