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

        private string _GetHostFromUri(int port = 25565)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            // urlPath = "https://krokodilgame.ru/";
            // return $"ws://127.0.0.1:{port}";
            return $"ws://192.168.1.25:25565";
#else
            return $"ws://192.168.1.25:25565";
            var urlPath = Application.absoluteURL;
            Uri uri = new Uri(urlPath);
            return System.IO.Path.Join($"wss://{uri.Host}", "ws");
#endif
        }

        private void _AddListeners()
        {
            _websocket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
                GameController.Instance.Invoke(nameof(GameController.GoToGameRoom), 0f);;
            };
            
            _websocket.OnMessage += (bytes) =>
            {
                string jsonString = System.Text.Encoding.UTF8.GetString(bytes);
                Packets packets = JsonUtility.FromJson<Packets>(jsonString);

                foreach (var packet in packets.events)
                {
                    switch (packet.eventCode)
                    {
                        // 
                        // PaintData
                        case 20:
                            _paintDatas.Add(JsonUtility.FromJson<PaintData>(packet.data));
                            break;
                        
                        // Chat
                        case 21:
                            GameController.Instance.NewMessage(JsonUtility.FromJson<ChatData>(packet.data));
                            break;
                        
                        // Chat
                        case 22:
                            GameController.Instance.SelectWord(JsonUtility.FromJson<SelectWordData>(packet.data));
                            break;
                        
                        // Start
                        case 30:
                            GameController.Instance.SetSpectate();
                            break;
                        
                        // Finish
                        case 31:
                            GameController.Instance.FinishThisGame(JsonUtility.FromJson<FinishData>(packet.data));
                            break;
                        
                        // Winner
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
        }

        public async Task Connect()
        {
            var url = _GetHostFromUri();
            Debug.Log($"WebSocket: trying to connect [{url}]");
            _websocket = new WebSocket(url);

            _AddListeners();

            // waiting for connection
            await _websocket.Connect();
        }

        // public async Task Connect(int port)
        // {
        //     var url = _GetHostFromUri(port);
        //     Debug.Log($"WebSocket: trying to connect [{url}]");
        //     _websocket = new WebSocket(url);
        //     
        //     _AddListeners();
        //     
        //     // waiting for connection
        //     await _websocket.Connect();
        // }
        
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
        public void SendPaintEvent(PaintData paintData)
        {
            _buffer.Add(new Packet()
            {
                eventCode = 20,
                data = JsonUtility.ToJson(paintData),
            });
        }

        public void SendMsgEvent(ChatData chatData)
        {
            _buffer.Add(new Packet()
            {
                eventCode = 21,
                data = JsonUtility.ToJson(chatData),
            });
        }

        public void SendConnected()
        {
            _buffer.Add(new Packet()
            {
                eventCode = 1,
                data = "{}",
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
            [SerializeField] public bool draw;
            [SerializeField] public string word;
        }

        [System.Serializable]
        public class FinishData
        {
            [SerializeField] public bool winner;
        }
        #endregion
    }
}
