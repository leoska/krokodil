using System;
using UnityEngine;

namespace Networking
{
    public class HttpController
    {
        
        
        [System.Serializable]
        public class HttpResponse
        {
            [SerializeField] public string data;
        }
    }
}