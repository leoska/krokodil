using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private NetworkController _networkController = new NetworkController();
    
    // Start is called before the first frame update
    async void Start()
    {
        await _networkController.Connect();
        
        await Task.Delay(1000);
        await _networkController.SendTestMessage();
    }

    // Update is called once per frame
    void Update()
    {
        _networkController.Update();
    }

    private async void OnApplicationQuit()
    {
        await _networkController.CloseConnection();
    }
}
