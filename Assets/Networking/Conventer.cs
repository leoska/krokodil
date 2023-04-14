using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Networking
{
    public class Conventer
    {
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
}