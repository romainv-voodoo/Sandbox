using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Voodoo.Network
{
    public static class WebRequest 
    {
        private static Dictionary<int, IWebRequestHandler> idToResult = new Dictionary<int, IWebRequestHandler>();

        public static (string, string) requestHeader;

        public static void Get(string url, IWebRequestHandler handler)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            SendAndCache(request, handler);
        }
        
        public static async Task<UnityWebRequest> GetAsync(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            await SendAndCacheAsync(request);
            return request;
        }

        public static void Put(string url, string content, IWebRequestHandler handler)
        {
            UnityWebRequest request = UnityWebRequest.Put(url, content);
            SendAndCache(request, handler);
        }

        public static async Task<UnityWebRequest> PutAsync(string url, string content)
        {
            UnityWebRequest request = new UnityWebRequest(url, "PUT", new DownloadHandlerBuffer(), new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                uploadHandler = { contentType = "application/json" }
            };

            await SendAndCacheAsync(request);
            return request;
        }

        public static void Post(string url, string content, IWebRequestHandler handler)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                uploadHandler = {contentType = "application/json"}
            };

            SendAndCache(request, handler);
        }
        
        public static async Task<UnityWebRequest> PostAsync(string url, string content)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                uploadHandler = {contentType = "application/json"}
            };

            await SendAndCacheAsync(request);
            return request;
        }

        public static void SendAndCache(UnityWebRequest request, IWebRequestHandler handler) 
        {
            SetRequestHeader(request);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            if (asyncOperation == null)
            {
                return;
            }

            idToResult.Add(asyncOperation.GetHashCode(), handler);
            asyncOperation.completed += OnAsyncOperationComplete;
        }

        public static async Task SendAndCacheAsync(UnityWebRequest request) 
        {
            SetRequestHeader(request);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            
            if (asyncOperation == null)
            {
                return;
            }
            
            await asyncOperation;
        }

        public static void SetRequestHeader(UnityWebRequest request)
        {
            if (requestHeader.Item2 == null || requestHeader.Item2.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < requestHeader.Item2.Length; i++)
            {
                char character = requestHeader.Item2[i];
                if (char.IsLetterOrDigit(character) == false && character != ' ')
                {
                    return;
                }
            }

            request.SetRequestHeader(requestHeader.Item1, requestHeader.Item2);
        }

        private static void OnAsyncOperationComplete(AsyncOperation operation)
        {
            UnityWebRequestAsyncOperation webOperation = operation as UnityWebRequestAsyncOperation;
            if (webOperation == null)
            {
                return;
            }

            IWebRequestHandler handler = idToResult.ContainsKey(webOperation.GetHashCode()) ? idToResult[webOperation.GetHashCode()] : null;
            if (handler == null)
            {
                return;
            }
            
            // if (webOperation.webRequest.responseCode >= 200 && webOperation.webRequest.responseCode <300)
            if (string.IsNullOrEmpty(webOperation.webRequest.error))
            {
                handler.OnSuccess(webOperation.webRequest);
            }
            else
            {
                handler.OnError(webOperation.webRequest);
            }

            webOperation.webRequest.Dispose();
            idToResult.Remove(operation.GetHashCode());
        }
    }
}