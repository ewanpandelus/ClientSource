using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using GameServer;
using System.Threading.Tasks;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int bufferSize = 1024;

    public string IP = "192.168.1.98";
    public int port = 44818;
    public int id;
    public static float ping;

    public TCP tcp;
    public UDP udp;
    private bool connected = false;
    private static Resend resend;


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
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
        DatagramSend.SetResendPackets();
        resend = GameObject.Find("GameManager").GetComponent<Resend>();
    }
    public void ConnectToServer()
    {
        connected = true;
        tcp.Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    public void Disconnect()
    {
        ClientSend.Disconnect();
        udp.Disconnect();
        if (connected)
        {
            connected = false;
        }
     

    }
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;
        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.IP), instance.port);
        }
        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);
            ClientSend.InitialPacket();
        }
     
        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Close();
            }
          
        }
        public void SendData(byte[] _packet)
        {
            try
            {
                if (socket != null)
                {
                    socket.BeginSend(_packet, _packet.Length, null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }


        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _packet = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_packet.Length < 4)
                {
                    Disconnect();
                    return;
                }
                ProcessPackets(_packet);
            }
            catch
            {
                Disconnect();
            }
        }
        private void ProcessPackets(byte[] _data)
        {
            ThreadManager.ProcessOnMainThread(() =>
            {
                DatagramReceiver.instance.ManageDatagram(_data);
            });
          
        }
    }


    public class TCP
    {
        public TcpClient socket;

        public void Connect()
        {
            socket = new TcpClient();
            try
            {
                socket.BeginConnect(instance.IP, instance.port, ConnectCallback, socket);
            }
            catch
            {
                GameManager.UpdateWaitText();
            }
   
        }
        private void Disconnect()
        {
            socket.Close();
            socket = null;

        }
        private void ConnectCallback(IAsyncResult _asyncResult)
        {
            try
            {
                socket.EndConnect(_asyncResult);
                Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            Disconnect();
        }
    }


 
    


}
