using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using NativeWebSocket;

namespace Networking
{
    public class WebSocketController
    {
        private WebSocket _websocket = null;
        private List<Packet> _buffer = new List<Packet>();

        private List<PaintData> _paintDatas = new List<PaintData>();

        private async Task Send(Packet packet)
        {
            if (_websocket.State == WebSocketState.Open)
            {
                string json = JsonUtility.ToJson(packet);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _websocket.Send(bytes);
            }
        }

        private void SendBuffer()
        {
            if (_buffer.Count < 1)
                return;
            
            if (_websocket.State == WebSocketState.Open)
            {
                
                string json = JsonUtility.ToJson(new Packets()
                {
                    events = _buffer,
                });
                
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                _websocket.Send(bytes);
            }

            _buffer.Clear();
        }

        private string _GetHostFromUri()
        {
            var urlPath = Application.absoluteURL;
#if !UNITY_WEBGL || UNITY_EDITOR
            urlPath = "https://krokodilgame.ru/";
#endif
            Uri uri = new Uri(urlPath);
            return System.IO.Path.Join($"wss://{uri.Host}", "ws");
        }

        public async Task Connect()
        {
            var url = _GetHostFromUri();
            Debug.Log($"WebSocket: trying to connect [{url}]");
            _websocket = new WebSocket(url);

            _websocket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
            };
            
            _websocket.OnMessage += (bytes) =>
            {
                string jsonString = System.Text.Encoding.UTF8.GetString(bytes);
                Packets packets = JsonUtility.FromJson<Packets>(jsonString);

                foreach (var packet in packets.events)
                {
                    switch (packet.eventCode)
                    {
                        case 20:
                            _paintDatas.Add(JsonUtility.FromJson<PaintData>(packet.data));
                            break;
                    }
                }

                if (_paintDatas.Count > 0)
                {
                    GameController.Instance.paintCanvas.OtherDraw(_paintDatas.ToArray());
                    _paintDatas.Clear();
                }
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

        public void Tick()
        {
            SendBuffer();
        }

        public void DispatchMessageQueue()
        {
    #if !UNITY_WEBGL || UNITY_EDITOR
            if (_websocket == null)
                return;
            
            if (_websocket.State != WebSocketState.Open)
                return;
            
            _websocket.DispatchMessageQueue();
    #endif
        }
        
        public async Task CloseConnection()
        {
            if (_websocket == null)
                return;
            
            await _websocket.Close();
        }
        
        #region Methods for send Data Packets
        public async Task SendPaintEvent(PaintData paintData)
        {
            _buffer.Add(new Packet()
            {
                eventCode = 20,
                data = JsonUtility.ToJson(paintData),
            });
        }
        #endregion

        #region Packets

        public class Packets
        {
            [SerializeField] public List<Packet> events;
        }
        
        // Packet class
        [System.Serializable]
        public class Packet
        {
            [SerializeField] public int eventCode;
            [SerializeField] public string data;
            [SerializeField] public int stamp;
        }

        // Data for connection event (code 1)
        [System.Serializable]
        public class ConnectionData
        {
            public string nickName;
        }

        // Data for draw event (code 20)
        [System.Serializable]
        public class PaintData
        {
            [SerializeField] public Vector2 pointStart;
            [SerializeField] public Vector2 pointEnd;
            
            [SerializeField] public int x;
            [SerializeField] public int y;
            [SerializeField] public int diameter;
            [SerializeField] public PaintCanvas.Brush brushType;
            [SerializeField] public Color color;
        }
        
        // Data for chat event (code 21)
        [System.Serializable]
        public class ChatData
        {
            [SerializeField] public string chatString;
        }

        // Data for draw event (code 22)
        [System.Serializable]
        public class SelectWordData
        {
            [SerializeField] public int state = 0;
        }
        #endregion
    }
}