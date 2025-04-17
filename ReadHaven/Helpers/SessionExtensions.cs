using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ReadHaven.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            session.SetString(key, jsonString);
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var jsonString = session.GetString(key);
            return jsonString == null ? default(T) : JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
