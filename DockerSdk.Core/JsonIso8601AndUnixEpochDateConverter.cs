//using System;
//using System.Globalization;
//using Newtonsoft.Json;

//namespace DockerSdk.Core
//{
//    using System.Reflection;

//    internal class JsonIso8601AndUnixEpochDateConverter : JsonConverter
//    {
//        private static readonly DateTime UnixEpochBase = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof (DateTime) || objectType == typeof (DateTime?);
//        }

//        public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }

//        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
//        {
//            var isNullableType = (objectType.GetTypeInfo().IsGenericType && objectType.GetGenericTypeDefinition() == typeof (Nullable<>));
//            var value = reader.Value;

//            DateTime? result = value switch
//            {
//                null => null,
//                DateTime time => time,
//                string s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
//                long l => UnixEpochBase.AddSeconds(l),
//                _ => throw new NotImplementedException($"Deserializing {value.GetType().FullName} back to {objectType.FullName} is not handled.")
//            };

//            if (isNullableType && result == default(DateTime))
//            {
//                return null; // do not set result on DateTime? field
//            }

//            return result;
//        }
//    }
//}
