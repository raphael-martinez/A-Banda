using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Playmove.Core.API
{
    /// <summary>
    /// Wrapper to make unity webrequests more simple for our DevKit APIs accesses
    /// </summary>
    public class WebRequestWrapper : MonoBehaviour
    {
        private static WebRequestWrapper _instance;
        public static WebRequestWrapper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<WebRequestWrapper>();
                if (_instance == null)
                    _instance = new GameObject("WebRequestWrapper").AddComponent<WebRequestWrapper>();
                return _instance;
            }
        }

        void Awake()
        {
            if (this != Instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Make a GET request to SOP to a relative url and to the latest API version
        /// </summary>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Get(string relativeUrl, AsyncCallback<DownloadHandler> callback)
        {
            Get(Playtable.Instance.APIVersions, relativeUrl, callback);
        }
        /// <summary>
        /// Make a GET request to SOP to a relative url and to a specific API version
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Get(string apiVersion, string relativeUrl, AsyncCallback<DownloadHandler> callback)
        {
            StartCoroutine(GetRoutine(apiVersion, relativeUrl, callback));
        }
        /// <summary>
        /// Make a GET request to SOP to a relative url and to the latest API version
        /// passing any amount of parameters you want
        /// </summary>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="parameters">Parameters you need to pass to service</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Get(string relativeUrl, Dictionary<string, string> parameters, AsyncCallback<DownloadHandler> callback)
        {
            Get(Playtable.Instance.APIVersions, relativeUrl, parameters, callback);
        }
        /// <summary>
        /// Make a GET request to SOP to a relative url and to a specific API version
        /// passing any amount of parameters you want
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="parameters">Parameters you need to pass to service</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Get(string apiVersion, string relativeUrl, Dictionary<string, string> parameters, AsyncCallback<DownloadHandler> callback)
        {
            string param = string.Join("&", parameters.Select((element) => $"{element.Key}={element.Value}"));
            StartCoroutine(GetRoutine(apiVersion, $"{relativeUrl}?{param}", callback));
        }

        /// <summary>
        /// Make a POST request to SOP to a relative url and to the latest API version
        /// </summary>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="data">Data as JSON/STRING that you want to send</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Post(string relativeUrl, string data, AsyncCallback<DownloadHandler> callback)
        {
            Post(Playtable.Instance.APIVersions, relativeUrl, data, callback);
        }

        /// <summary>
        /// Make a POST request to SOP to a relative url and to a specific API version
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="data">Data as JSON/STRING that you want to send</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        public void Post(string apiVersion, string relativeUrl, string data, AsyncCallback<DownloadHandler> callback)
        {
            StartCoroutine(PostRoutine(apiVersion, relativeUrl, data, callback));
        }

        /// <summary>
        /// Unity coroutine to make the GET request to SOP services
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <param name="relativeUrl">Relative url with all parameters</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        /// <returns></returns>
        private IEnumerator GetRoutine(string apiVersion, string relativeUrl, AsyncCallback<DownloadHandler> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(Playtable.Instance.APIBaseURL + apiVersion + relativeUrl);
            SetDefaultHeaders(request);
            yield return request.SendWebRequest();

            bool hasError = request.isHttpError || request.isNetworkError;
            callback?.Invoke(new AsyncResult<DownloadHandler>()
            {
                Data = request.downloadHandler,
                Error = !hasError ? string.Empty : $"{request.error} | URL: {request.url}"
            });
        }
        /// <summary>
        /// Unity coroutine to make the POST request to SOP services
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <param name="relativeUrl">Relative url to SOP services</param>
        /// <param name="data">Data as JSON/STRING that you want to send</param>
        /// <param name="callback">Callback with DownloadHandler when completed</param>
        /// <returns></returns>
        private IEnumerator PostRoutine(string apiVersion, string relativeUrl, string data, AsyncCallback<DownloadHandler> callback)
        {
            UnityWebRequest request = new UnityWebRequest(Playtable.Instance.APIBaseURL + apiVersion + relativeUrl, "POST")
            {
                downloadHandler = new DownloadHandlerBuffer(),
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data))
            };
            SetDefaultHeaders(request);
            yield return request.SendWebRequest();

            bool hasError = request.isHttpError || request.isNetworkError;
            callback?.Invoke(new AsyncResult<DownloadHandler>()
            {
                Data = request.downloadHandler,
                Error = !hasError ? string.Empty : $"{request.error} | URL: {request.url}"
            });
        }

        /// <summary>
        /// Default headers for all requests made to SOP
        /// </summary>
        /// <param name="request">UnityWebRequest to set the headers</param>
        private void SetDefaultHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            if (!string.IsNullOrEmpty(Playtable.Instance.Key))
                request.SetRequestHeader("access_token", Playtable.Instance.Key);
        }
    }
}
