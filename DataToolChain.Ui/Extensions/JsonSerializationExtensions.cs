﻿using Newtonsoft.Json;

namespace JustTransformPlayground.Ui
{
    public static class JsonSerializationExtensions
    {
        public static TObject ToObject<TObject>(this string jsonString, bool ignoreNonSerializableClassReferences = false)
        {
            return JsonConvert.DeserializeObject<TObject>(jsonString, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) => {
                    args.ErrorContext.Handled = true;
                }
            });
        }

        public static object ToObject(this string jsonString, bool ignoreNonSerializableClassReferences = false)
        {
            return JsonConvert.DeserializeObject(jsonString, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) => {
                    args.ErrorContext.Handled = true;
                }
            });
        }

        public static string ToJson(this object serializableObject, bool indent = false, bool ignoreNonSerializableClassReferences = false)
        {
            return JsonConvert.SerializeObject(serializableObject, indent ? Formatting.Indented : Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (sender, args) => { args.ErrorContext.Handled = true; }
                });
        }
    }
}