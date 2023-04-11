using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;

using NativeWebSocket;

public class NetworkController
{
    private WebSocket _websocket = null;

    public async Task Connect()
    {
        _websocket = new WebSocket("ws://192.168.1.25:8081");

        _websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };
        
        _websocket.OnMessage += (bytes) =>
        {
            GameController.Instance.paintCanvas.OtherDraw(_FromBytes(bytes));
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

    public async Task SendPaintData(PacketData data)
    {
        if (_websocket.State == WebSocketState.Open)
        {
            await _websocket.Send(_GetBytes(data));
        }
    }

    public async Task CloseConnection()
    {
        await _websocket.Close();
    }
    
    private byte[] _GetBytes(PacketData str) {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
    
    private PacketData _FromBytes(byte[] arr)
    {
        PacketData str = new PacketData();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (PacketData)Marshal.PtrToStructure(ptr, str.GetType());
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return str;
    }
    
    public struct PacketData
    {
        public int x;
        public int y;
        public int diameter;
        public PaintCanvas.Brush brushType;
        public Color color;
    }
}
