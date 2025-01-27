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
    public class MultiPolygonConverter : JsonConverter
    {
        #region Public Properties

        public override bool CanRead => true;

        public override bool CanWrite => true;

        #endregion

        #region Public Methods

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MultiPolygon);
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

            var polygonsCoordinates = token.GetValue("coordinates", StringComparison.OrdinalIgnoreCase).ToObject<IEnumerable<IEnumerable<IEnumerable<Coordinate>>>>(serializer);

            // Take this array of arrays of arrays and create linear rings
            // and use those to create create polygons

            var polygons = polygonsCoordinates.Select(linearRings => new Polygon(linearRings.Select(linearRingCoordinates => new LinearRing(linearRingCoordinates))));

            return new MultiPolygon(polygons);
        }

        /// <summary>
        /// This flattens the coordinates property into an array of arrays of arrays
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MultiPolygon mp = (MultiPolygon)value;

            JToken.FromObject(new
            {
                type = mp.Type,
                coordinates = mp.Polygons.Select(polygon => polygon.LinearRings.Select(linearRing => linearRing.Points.Select(p => p.Coordinates)))
            }).WriteTo(writer);
        }

        #endregion
    }
}
