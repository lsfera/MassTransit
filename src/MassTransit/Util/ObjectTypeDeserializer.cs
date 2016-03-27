﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Util
{
    using System;
    using System.Collections.Generic;
    using Context;
    using Courier;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    /// <summary>
    /// Support deserialization of 'objects' from messages into actual types. Objects should have been
    /// serialized with JSON.NET (or some similar serializer).
    /// </summary>
    public class ObjectTypeDeserializer :
        IObjectTypeDeserializer
    {
        readonly JsonSerializer _deserializer;

        public ObjectTypeDeserializer(JsonSerializer deserializer)
        {
            _deserializer = deserializer;
        }

        public static IObjectTypeDeserializer Instance => Cached.Instance.Value;

        T IObjectTypeDeserializer.Deserialize<T>(IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            object value;
            if (!dictionary.TryGetValue(key, out value) && !TryGetValueCamelCase(key, dictionary, out value))
                return defaultValue;
//                throw new KeyNotFoundException($"The key was not present: {key}");

            return Deserialize<T>(value);
        }

        T IObjectTypeDeserializer.Deserialize<T>(IDictionary<string, object> dictionary, string key)
        {
            object value;
            if (!dictionary.TryGetValue(key, out value) && !TryGetValueCamelCase(key, dictionary, out value))
                throw new KeyNotFoundException($"The key was not present: {key}");

            return Deserialize<T>(value);
        }

        T IObjectTypeDeserializer.Deserialize<T>(IHeaderProvider dictionary, string key, T defaultValue)
        {
            object value;
            if (!dictionary.TryGetHeader(key, out value))
                return defaultValue;
            //                throw new KeyNotFoundException($"The key was not present: {key}");

            return Deserialize<T>(value);
        }

        public T Deserialize<T>(object value)
        {
            return (T)Deserialize(value, typeof(T), false);
        }

        T IObjectTypeDeserializer.Deserialize<T>(object value, T defaultValue)
        {
            var result = Deserialize(value, typeof(T), true);
            if (result == null)
                return defaultValue;

            return (T)result;
        }

        public object Deserialize(object value, Type objectType, bool allowNull = false)
        {
            JToken token = value as JToken ?? new JValue(value);
            if (token.Type == JTokenType.Null && allowNull)
                return null;

            using (var jsonReader = new JTokenReader(token))
                return SerializerCache.Deserializer.Deserialize(jsonReader, objectType);
        }

        public static T Deserialize<T>(IDictionary<string, object> dictionary, string key)
        {
            return Cached.Instance.Value.Deserialize<T>(dictionary, key);
        }

        public static T Deserialize<T>(IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            return Cached.Instance.Value.Deserialize(dictionary, key, defaultValue);
        }

        public static T Deserialize<T>(IHeaderProvider dictionary, string key, T defaultValue)
        {
            return Cached.Instance.Value.Deserialize(dictionary, key, defaultValue);
        }

        public static T Deserialize<T>(object value, T defaultValue)
        {
            return Cached.Instance.Value.Deserialize(value, defaultValue);
        }

        static bool TryGetValueCamelCase(string key, IDictionary<string, object> dictionary, out object value)
        {
            if (char.IsUpper(key[0]))
            {
                char[] chars = key.ToCharArray();
                chars[0] = char.ToLower(chars[0]);

                key = new string(chars);
                return dictionary.TryGetValue(key, out value);
            }

            value = null;
            return false;
        }


        static class Cached
        {
            internal static readonly Lazy<IObjectTypeDeserializer> Instance =
                new Lazy<IObjectTypeDeserializer>(() => new ObjectTypeDeserializer(SerializerCache.Deserializer));
        }
    }


    public interface IObjectTypeDeserializer
    {
        T Deserialize<T>(IDictionary<string, object> dictionary, string key, T defaultValue);
        T Deserialize<T>(IHeaderProvider headerProvider, string key, T defaultValue);

        T Deserialize<T>(IDictionary<string, object> dictionary, string key);

        T Deserialize<T>(object value);
        T Deserialize<T>(object value, T defaultValue);
        object Deserialize(object value, Type objectType, bool allowNull = false);
    }
}