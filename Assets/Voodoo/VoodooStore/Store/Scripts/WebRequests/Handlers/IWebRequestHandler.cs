using UnityEngine.Networking;

namespace Voodoo.Store
{
    public interface IWebRequestHandler
    {
        void OnSuccess(UnityWebRequest webRequest);

        void OnError(UnityWebRequest webRequest);
    }
}