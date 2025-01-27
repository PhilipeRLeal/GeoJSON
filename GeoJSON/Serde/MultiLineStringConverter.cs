﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BAMCIS.GeoJSON.Serde
{
    /// <summary>
    /// Converts a MultiLineString GeoJSON object to and from JSON
    /// </summary>
    public class MultiLineStringConverter : JsonConverter
    {
        #region Public Properties

        public override bool CanRead => true;

        public override bool CanWrite => true;

        #endregion

        #region Public Methods

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MultiLineString);
        }

        /// <summary>
        /// This takes the array of arrays and recasts them back to line string objects
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject token = JObject.Load(reader);

            IEnumerable<IEnumerable<Coordinate>> coordinates = token.GetValue("Coordinates", StringComparison.OrdinalIgnoreCase).ToObject<IEnumerable<IEnumerable<Coordinate>>>(serializer);

            List<LineString> lineStrings = coordinates.Select(c => LineSegment.CoordinatesToLineString(c)).ToList();

            return new MultiLineString(lineStrings);
        }

        /// <summary>
        /// This flattens the coordinates property into an array of arrays
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MultiLineString mls = (MultiLineString)value;

            JToken.FromObject(new
            {
                type = mls.Type,
                coordinates = mls.LineStrings.Select(x => x.Points.Select(p => p.Coordinates))
            }).WriteTo(writer);
        }

        #endregion
    }
}
