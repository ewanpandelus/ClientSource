using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
public class DatagramReceiver : MonoBehaviour
{
    public static DatagramReceiver instance;
    public static bool welcomed = false;

    public struct WelcomeReceived
    {
        public int id;
        public int packetType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string msg;
        public int ackNumber;
    }
    public struct SpawnPlayer
    {
        public int id;
        public int packetType;
        public int playerID;
        public Vector3 position;
        public Quaternion rotation;
        public int ackNumber;
    }
    public struct PlayerPosition
    {
        public int id;
        public int packetType;
        public float elapsedTime;
        public Vector3 position;
    }
    public struct PlayerRotation
    {
        public int id;
        public int packetType;
        public float xRotation;
        public bool leftOf;
    }
    public struct ServerPing
    {
        public int id;
        public int packetType;
        public int ackNumber;
    }
    public struct ClientPing
    {
        public int id;
        public int packetType;
        public int ackNumber;
        public float ping;
    }
    public struct Ack
    {
        public int id;
        public int packetType;
        public int ackNumber;
    }
    public struct Jump
    {
        public int id;
        public int packetType;
    }
    public struct Shoot
    {
        public int id;
        public int packetType;
    }
    public struct BulletCollision
    {
        public int id;
        public int packetType;
        public Vector3 direction;
        public int ackNumber;
    }
    public struct Disonnect
    {
        public int id;
        public int packetType;
        public int ackNumber;
    }

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
    public void ManageDatagram(byte[] _datagram)
    {
        int _packetType = BitConverter.ToInt32(_datagram.Skip(4).Take(4).ToArray(), 0);
  

        if (_packetType == 0)
        {
            ManageWelcomeDatagram(_datagram);
            Debug.Log("Welcome");
            return;
        }
        if (_packetType == 1)
        {
            ManageSpawnDatagram(_datagram);
            Debug.Log("Spawn");
            return;
        }
        if(_packetType == 2)
        {
            Debug.Log("Position");
            ManagePositionDatagram(_datagram);
            return;
        }
        if (_packetType == 3)
        {

            ManageRotationDatagram(_datagram);
            return;
        }
        if (_packetType == 4)
        {
            ManagePingDatagram(_datagram);
            return;
        }
        if (_packetType == 5)
        {
            Debug.Log("Jump");
            ManageJumpDatagram(_datagram);
            return;
        }
        if (_packetType == 6)
        {
            Debug.Log("Shoot");
            ManageShootDatagram(_datagram);
            return;
        }
        if (_packetType == 7)
        {
            ManageClientPingDatagram(_datagram);
            return;
        }
        if (_packetType == 8)
        {
            ManageBulletCollisionDatagram(_datagram);
            return;
        }
        if (_packetType == 9)
        {
            ManageDisconnectDatagram(_datagram);
            return;
        }
        if (_packetType == 10)
        {
            ManageAckDatagram(_datagram);
            return;
        }
    }
    private void ManageWelcomeDatagram(byte[] _datagram)
    {
        if (!welcomed)
        {
            WelcomeReceived welcome = BytesToStructs.FromBytesToWelcomeReceived(_datagram);
            string msg = welcome.msg;
            int id = welcome.id;
            int ackNumber = welcome.ackNumber;
            Debug.Log($"Message from server: {msg}.");
            Client.instance.id = id;
            ClientSend.WelcomeReceived(ackNumber);
            DatagramSend.UpdatePacketsDictionary(0);
            welcomed = true;
        }
 
    }
    private void ManageSpawnDatagram(byte[] _datagram)
    {
        SpawnPlayer spawnPlayer = BytesToStructs.FromBytesToSpawnPlayer(_datagram);
        Vector3 position = spawnPlayer.position;
        int id = spawnPlayer.playerID;
        GameManager.instance.SpawnPlayer(id, "Gerald", position, spawnPlayer.rotation);
        ClientSend.Ack(spawnPlayer.ackNumber);
    }
    private void ManagePositionDatagram(byte[] _datagram)
    {

        try
        {
            PlayerPosition position = BytesToStructs.FromBytesToPosition(_datagram);
            Vector3 pos = position.position;
            int id = position.id;
            float _elapsedTime = position.elapsedTime;
            GameManager.players[id].GetComponent<Prediction>().SetPosition(pos, _elapsedTime);
         
        }
        catch(Exception ex)
        {
            Debug.Log($"Position sending before player has spawned {ex}");
        }
       
    }
    private void ManageRotationDatagram(byte[] _datagram)
    {
        GameManager.players[Client.instance.id].GetComponent<PlayerController>().SetTimeSinceLastPacket(0);
        try
        {
            PlayerRotation rotation = BytesToStructs.FromBytesToRotation(_datagram);
            int id = rotation.id;
            GameManager.players[id].GetComponent<Prediction>().SetRotation(rotation.xRotation, rotation.leftOf);

        }
        catch (Exception ex)
        {
            Debug.Log($"Rotation sending before player has spawned {ex}");
        }
       
    }
    private void ManagePingDatagram(byte[] _datagram)
    {
        ServerPing ping = BytesToStructs.FromBytesToPing(_datagram);
        ClientSend.Ping(ping.ackNumber);
   
    }
    private void ManageJumpDatagram(byte[] _datagram)
    {
   
        Jump jump = BytesToStructs.FromBytesToJump(_datagram);
        GameManager.players[jump.id].GetComponent<Prediction>().StartJump();
    }
    private void ManageShootDatagram(byte[] _datagram)
    {

        Shoot shoot = BytesToStructs.FromBytesToShoot(_datagram);
        GameManager.players[shoot.id].GetComponent<Prediction>().StartShooting();
    }
    private void ManageClientPingDatagram(byte[] _datagram)
    {
        ClientPing clientPing = BytesToStructs.FromBytesToClientPing(_datagram);
        ClientSend.Ack(clientPing.ackNumber);
        GameManager.ping = clientPing.ping;
    }
    private void ManageBulletCollisionDatagram(byte[] _datagram)
    {
        BulletCollision bulletCollision = BytesToStructs.FromBytesToBulletCollision(_datagram);
        if (GameManager.players[bulletCollision.id].GetComponent<PlayerController>() != null)
        {
            GameManager.players[bulletCollision.id].GetComponent<PlayerController>().CollisionScale();
        }
        else
        {
            GameManager.players[bulletCollision.id].GetComponent<Prediction>().CollisionScale();
        }
        ClientSend.Ack(bulletCollision.ackNumber);
    }
    private void ManageDisconnectDatagram(byte[] _datagram)
    {
        Disonnect disonnect = BytesToStructs.FromBytesToDisconnect(_datagram);
        ClientSend.DisconnectReceived(disonnect.ackNumber);
        GameManager.players[Client.instance.id].GetComponent<PlayerController>().SetConnected(false);
        GameManager.DisconnectPlayer();
    }
    private void ManageAckDatagram(byte[] _datagram)
    {
        Ack ack = BytesToStructs.FromBytesToAck(_datagram);
        DatagramSend.UpdatePacketsDictionary(ack.ackNumber);
    }
}
public static class BytesToStructs
{
    public static DatagramReceiver.PlayerRotation FromBytesToRotation(byte[] arr)
    {
        DatagramReceiver.PlayerRotation str = new DatagramReceiver.PlayerRotation();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.PlayerRotation)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }

    public static DatagramReceiver.PlayerPosition FromBytesToPosition(byte[] arr)
    {
        DatagramReceiver.PlayerPosition str = new DatagramReceiver.PlayerPosition();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.PlayerPosition)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.WelcomeReceived FromBytesToWelcomeReceived(byte[] arr)
    {
        DatagramReceiver.WelcomeReceived str = new DatagramReceiver.WelcomeReceived();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.WelcomeReceived)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.SpawnPlayer FromBytesToSpawnPlayer(byte[] arr)
    {
        DatagramReceiver.SpawnPlayer str = new DatagramReceiver.SpawnPlayer();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.SpawnPlayer)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.ServerPing FromBytesToPing(byte[] arr)
    {
        DatagramReceiver.ServerPing str = new DatagramReceiver.ServerPing();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.ServerPing)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.Jump FromBytesToJump(byte[] arr)
    {
        DatagramReceiver.Jump str = new DatagramReceiver.Jump();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.Jump)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.ClientPing FromBytesToClientPing(byte[] arr)
    {
        DatagramReceiver.ClientPing str = new DatagramReceiver.ClientPing();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.ClientPing)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    public static DatagramReceiver.Shoot FromBytesToShoot(byte[] arr)
    {
        DatagramReceiver.Shoot str = new DatagramReceiver.Shoot();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.Shoot)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);
        return str;
    }
    public static DatagramReceiver.BulletCollision FromBytesToBulletCollision(byte[] arr)
    {
        DatagramReceiver.BulletCollision str = new DatagramReceiver.BulletCollision();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.BulletCollision)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);
        return str;
    }
    public static DatagramReceiver.Disonnect  FromBytesToDisconnect(byte[] arr)
    {
        DatagramReceiver.Disonnect str = new DatagramReceiver.Disonnect();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.Disonnect)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);
        return str;
    }
    public static DatagramReceiver.Ack FromBytesToAck(byte[] arr)
    {
        DatagramReceiver.Ack str = new DatagramReceiver.Ack();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (DatagramReceiver.Ack)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);
        return str;
    }
}