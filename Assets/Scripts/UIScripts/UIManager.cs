using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField inputField;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Two Clients! Eeek, destroying one of them");
            Destroy(this);
        }
    }
    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        inputField.interactable = false;
        Client.instance.IP = inputField.text.ToString();
        Client.instance.ConnectToServer();
    }
    public void ReEntry()
    {
        startMenu.SetActive(true);
        inputField.interactable = true;
    }
}
