using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatController : MonoBehaviour
{
    public int maxSize = 25;
    public List<GameObject> messages = new List<GameObject>();
    public GameObject chatMessage = null;
    public GameObject chatContent = null;

    private void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        foreach (var obj in messages)
        {
            Destroy(obj.gameObject);
        }
        
        messages.Clear();
    }
    
    public void MessageAdd(string msg)
    {
        if (messages.Count > maxSize)
        {
            Destroy(messages[0].gameObject);
            messages.RemoveAt(0);
        }

        var chatString = Instantiate(chatMessage, chatContent.transform);

        chatString.GetComponent<TextMeshProUGUI>().text = msg;
        
        messages.Add(chatString);
    }
}
