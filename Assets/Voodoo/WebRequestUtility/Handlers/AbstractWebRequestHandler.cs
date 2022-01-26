using UnityEngine.Networking;

namespace Voodoo.Network
{
	public abstract class AbstractWebRequestHandler : IWebRequestHandler
	{
		public abstract void OnSuccess(UnityWebRequest webRequest);
        
		public virtual void OnError(UnityWebRequest webRequest)
		{
#if DEBUG_WEB_REQUEST
        DebugHelper.DumpWebResponse(GetType().Name + "Errors.json", webRequest.downloadHandler.text);
#endif

			UnityEngine.Debug.LogError("unity web request with url : " + webRequest.url + " " + webRequest.responseCode.ToString() + "\nFailed with message : \n" + webRequest.error);
		}
	}
}