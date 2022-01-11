using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class DatagramSend : MonoBehaviour
{
    public DatagramSend instance;
    private static Resend resend;
    public static Dictionary<int, bool> sentPackets = new Dictionary<int, bool>();
    public static Dictionary<int, byte[]> resendPacketsContent = new Dictionary<int, byte[]>();
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
    public struct WelcomeReceived
    {
        public int id;
        public int packetType;
        public int serverAck;
        public int clientAck;
    }
    public struct Rotation
    {
        public int id;
        public int packetType;
        public float rotX;
        public bool leftOf;
    }
    public struct Position
    {
        public int id;
        public int packetType;
        public float elapsedTime;
        public Vector3 position;
    }
    public struct Ping
    {
        public int id;
        public int packetType;
        public int ackNumber;
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
    public struct Disconnect
    {
        public int id;
        public int packetType;
        public int ackNumber;
    }
    public struct InitialPacket
    {
        public int ackNumber;
    }
    public static void SetResendPackets()
    {
        resend = GameObject.Find("GameManager").GetComponent<Resend>();
    }
    public static void UpdatePacketsDictionary(int packetNo)
    {
        sentPackets[packetNo] = true;
    }

    public static void AddToPacketsDictionary(int packetNo, byte[] packet, int resendCount)
    {
        sentPackets.Add(packetNo, false);
        resendPacketsContent.Add(packetNo, packet);
        if (resendCount > 3)
        {
            ThreadManager.ProcessOnMainThread(() =>
            {
                resend.StartResending(packetNo);
            });
        
        }
        else
        {
            ThreadManager.ProcessOnMainThread(() =>
            {
                resend.StartResending(packetNo,resendCount);
            });
        }
    }

    public class GenericDatagramCreator<T>
    {
        public byte[] GetBytes<T1>(T1 str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

    }
}

