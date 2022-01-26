using System.Threading.Tasks;
using UnityEngine;

namespace Voodoo.Store
{
    public static class SlackWebHook
    {
        public static async Task PostToSlack(string hookUrl, string json)
        {
            WebRequest.requestHeader = ("Content-Type", "application/json");
            await WebRequest.PostAsync(hookUrl, json);
        }
        
        public static async Task PostToSlack<T>(string hookUrl, T obj)
        {
            await PostToSlack(hookUrl, JsonUtility.ToJson(obj));
        }
    }
}