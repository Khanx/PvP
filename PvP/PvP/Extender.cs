using Newtonsoft.Json.Linq;
using Pipliz;
using System.Collections.Generic;

namespace PvP
{
    static class Extender
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;

            return defaultValue;
        }

        public static JValue GetAsOrDefault<JValue>(this JToken jToken, string propertyName, JValue defaultValue)
        {
            JValue value = jToken.Value<JValue>(propertyName);

            if (default(JValue).Equals(value))
                return value;

            return defaultValue;
        }
       

        public static Players.Player GetPlayer(Players.PlayerIDShort playerIDShort)
        {
            Players.TryGetPlayer(playerIDShort, out Players.Player plr);

            return plr;
        }
    }
}
