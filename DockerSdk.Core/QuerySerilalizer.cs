//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DockerSdk.Core
//{
//    public static class QuerySerilalizer
//    {
//        public static string Serialize(object? queryObject)
//        {
//            if (queryObject == null)
//                return "";

//            var sb = new StringBuilder();

//            var type = queryObject.GetType();
//            if (!serializers.TryGetValue(type, out var serializer))
//                serializer = serializers[type] = MakeSerializer(type);

//            return serializer(queryObject);
//        }

//        private static Func<object, string> MakeSerializer(Type type)
//        {
            
//        }

//        private static readonly Dictionary<Type, Func<object, string>> serializers = new();
//    }

//    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
//    public sealed class QueryParameterNameAttribute : Attribute
//    {
//        public QueryParameterNameAttribute(string name) => Name = name;

//        public string Name { get; }
//    }
//}
