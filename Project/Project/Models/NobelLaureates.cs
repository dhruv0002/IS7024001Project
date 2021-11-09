﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickTypeNobelLaureates;
//
//    var nobelLaureates = NobelLaureates.FromJson(jsonString);

namespace QuickTypeNobelLaureates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class NobelLaureates
    {
        [JsonProperty("laureates")]
        public List<Laureate> Laureates { get; set; }
    }

    public partial class Laureate
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [DisplayName("First Name")]
        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("born")]
        public string Born { get; set; }

        [JsonProperty("died")]
        public string Died { get; set; }

        [DisplayName("Born Country")]
        [JsonProperty("bornCountry")]
        public string BornCountry { get; set; }

        [DisplayName("Born Country Code")]
        [JsonProperty("bornCountryCode")]
        public string BornCountryCode { get; set; }

        [DisplayName("Born City")]
        [JsonProperty("bornCity")]
        public string BornCity { get; set; }

        [DisplayName("Died Country")]
        [JsonProperty("diedCountry", NullValueHandling = NullValueHandling.Ignore)]
        public string DiedCountry { get; set; }

        [DisplayName("Died Country Code")]
        [JsonProperty("diedCountryCode", NullValueHandling = NullValueHandling.Ignore)]
        public string DiedCountryCode { get; set; }

        [DisplayName("Died City")]
        [JsonProperty("diedCity", NullValueHandling = NullValueHandling.Ignore)]
        public string DiedCity { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("prizes")]
        public List<Prize> Prizes { get; set; }
    }

    public partial class Prize
    {
        [JsonProperty("year")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Year { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("share")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Share { get; set; }

        [JsonProperty("motivation")]
        public string Motivation { get; set; }

        [JsonProperty("affiliations")]
        public List<AffiliationElement> Affiliations { get; set; }
    }

    public partial class AffiliationClass
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public partial struct AffiliationElement
    {
        public AffiliationClass AffiliationClass;
        public List<object> AnythingArray;

        public static implicit operator AffiliationElement(AffiliationClass AffiliationClass) => new AffiliationElement { AffiliationClass = AffiliationClass };
        public static implicit operator AffiliationElement(List<object> AnythingArray) => new AffiliationElement { AnythingArray = AnythingArray };
    }

    public partial class NobelLaureates
    {
        public static NobelLaureates FromJson(string json) => JsonConvert.DeserializeObject<NobelLaureates>(json, QuickTypeNobelLaureates.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this NobelLaureates self) => JsonConvert.SerializeObject(self, QuickTypeNobelLaureates.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                AffiliationElementConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class AffiliationElementConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AffiliationElement) || t == typeof(AffiliationElement?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<AffiliationClass>(reader);
                    return new AffiliationElement { AffiliationClass = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<List<object>>(reader);
                    return new AffiliationElement { AnythingArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type AffiliationElement");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (AffiliationElement)untypedValue;
            if (value.AnythingArray != null)
            {
                serializer.Serialize(writer, value.AnythingArray);
                return;
            }
            if (value.AffiliationClass != null)
            {
                serializer.Serialize(writer, value.AffiliationClass);
                return;
            }
            throw new Exception("Cannot marshal type AffiliationElement");
        }

        public static readonly AffiliationElementConverter Singleton = new AffiliationElementConverter();
    }
}
