using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using NativeWebSocket;

public class NetworkController
{
    private WebSocket _websocket = null;

    public async Task Connect()
    {
        _websocket = new WebSocket("ws://127.0.0.1:8081");
        
        _websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };
        
        _websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            Debug.Log(bytes);

            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
        };
        
        _websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };
        
        // waiting for messages
        await _websocket.Connect();
    }

    public void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _websocket.DispatchMessageQueue();
#endif
    }

    public async Task SendTestMessage()
    {
        if (_websocket.State == WebSocketState.Open)
        {
            await _websocket.SendText("plain text message");
        }
    }

    public async Task CloseConnection()
    {
        await _websocket.Close();
    }
}
