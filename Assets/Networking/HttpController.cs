using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class HttpController
    {
        private static IEnumerator _GetRequest(UnityWebRequest req)
        {
            {
                yield return req.SendWebRequest();

                switch (req.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        throw new Exception($"Error: {req.error}");
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        throw new Exception($"HTTP Error: {req.error}");
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log($"Received: " + req.downloadHandler.text);
                        break;
                }
            }
        }

        public static IEnumerator GetGameInfo()
        {
            var urlPath = Application.absoluteURL;
#if !UNITY_WEBGL || UNITY_EDITOR
            // urlPath = "https://krokodilgame.ru/";
            urlPath = "http://localhost:8080";
#endif
            string uri = System.IO.Path.Join(urlPath, "api/master/getGameInfo");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                HttpController._GetRequest(webRequest);
                
                while (!webRequest.isDone)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                HttpResponse res = JsonUtility.FromJson<HttpResponse>(webRequest.downloadHandler.text);
                
                
                Debug.Log(res.response);
            }
        }

        IEnumerator PostRequestForm(string url)
        {
            WWWForm form = new WWWForm();
            form.AddField("myField", "myData");
            form.AddField("Game Name", "Mario Kart");

            UnityWebRequest uwr = UnityWebRequest.Post(url, form);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
            }
        }
        
        IEnumerator PostRequestJSON(string url, string json)
        {
            var uwr = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
            }
        }
        
        [System.Serializable]
        public class HttpResponse
        {
            [SerializeField] public Response response;
        }

        [System.Serializable]
        public class Response
        {
            [SerializeField] public GameInfo gameInfo;
        }

        [System.Serializable]
        public class GameInfo
        {
            [SerializeField] public int port;
        }
    }
}