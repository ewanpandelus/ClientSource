using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int,PlayerManager>();
    public GameObject serverGameObjectPrefab;
    public GameObject localGameObjectPrefab;
    public static GameObject[] playerGameObjects = new GameObject[2];
    public List<int> spawnedObjects = new List<int>();
    public static float ping = 0;
    [SerializeField] private GameObject bulletPrefab;
    private PlayerController playerController;
    private static GameObject waitText;
    private static int connectedPlayers;
    private static UIManager UIManager; 

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
        playerController = Instantiate(localGameObjectPrefab, new Vector3(2f,2f,0f), Quaternion.identity).GetComponent<PlayerController>();
        playerGameObjects[0] = playerController.gameObject;
        playerGameObjects[1] = Instantiate(serverGameObjectPrefab, new Vector3(-2f, 2f, 0f), Quaternion.identity);
        waitText = GameObject.Find("Wait");
        UIManager = GameObject.Find("Menu").GetComponent<UIManager>();
  
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        if (spawnedObjects.Contains(_id))
        {
            return;
        }
        if(_id == Client.instance.id)
        {
            SpawnPlayerOne(_position, _rotation, _id);
            spawnedObjects.Add(_id);
            connectedPlayers++;
           
        }
        else
        {
            SpawnPlayerTwo(_position,_rotation, _id);
            spawnedObjects.Add(_id);
            connectedPlayers++;
        }
        if (connectedPlayers == 2)
        {
            waitText.GetComponent<TextMeshProUGUI>().text = "";
            playerGameObjects[0].GetComponent<PlayerController>().SetConnected(true);
        }
        else waitText.GetComponent<TextMeshProUGUI>().text = "The game will start when two players are connected";

    }
    private void SpawnPlayerOne(Vector3 _position, Quaternion _rotation, int _id)
    {
        GameObject player = playerGameObjects[0];
        GameObject.Find("Main Camera").GetComponent<CameraFollow>().SetTarget(player.transform);
        playerController.gameObject.transform.position = _position;
        playerController.gameObject.transform.rotation = _rotation;
        playerController.gameObject.GetComponent<PlayerManager>().id = _id;
        players.Add(_id, playerController.gameObject.GetComponent<PlayerManager>());
    }
    private void SpawnPlayerTwo(Vector3 _position, Quaternion _rotation, int _id)
    {
        GameObject player = playerGameObjects[1];
        player.transform.position = _position;
        player.transform.rotation = _rotation;

        player.GetComponent<PlayerManager>().id = _id;
        player.GetComponent<Prediction>().Initialise();
        player.GetComponent<Prediction>().SetBulletPrefab(bulletPrefab);
        Physics.IgnoreCollision(player.GetComponent<Collider>(), playerGameObjects[0].GetComponent<Collider>());
        players.Add(_id, player.GetComponent<PlayerManager>());
    }
    public static void DisconnectPlayer()
    {
        playerGameObjects[0].transform.position = new Vector3(5f, 2.085f, 0);
        playerGameObjects[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
        playerGameObjects[1].transform.position = new Vector3(-5f, 2.085f, 0);
        waitText.GetComponent<TextMeshProUGUI>().text = "Other player disconnected, waiting for another connection";
        connectedPlayers--;
    }
    public static void UpdateWaitText()
    {
        waitText.GetComponent<TextMeshProUGUI>().text = "There is no server hosting on this IP, please re-enter";
        UIManager.ReEntry();
    }


}
