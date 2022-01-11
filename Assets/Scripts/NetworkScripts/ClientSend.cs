using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using static DatagramSend;
using Ping = DatagramSend.Ping;

public class ClientSend : MonoBehaviour
{
    public static int ack = 0;
    public static bool welcomed;

    public static void SendUDPData(byte[] _packet)
    {
        Client.instance.udp.SendData(_packet);
    }
    public static void WelcomeReceived(int serverAck)
    {
        GenericDatagramCreator<WelcomeReceived> genericDatagramCreatorRotation = new GenericDatagramCreator<WelcomeReceived>();
        WelcomeReceived _packet = new WelcomeReceived
        {
            packetType = 0,
            id = Client.instance.id,
            serverAck = serverAck,
            clientAck = ack
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        DatagramSend.AddToPacketsDictionary(ack, _pack, 5);
        SendUDPData(_pack);
        ack++;
    }

    public static void PlayerPosition(float elaspedTime)
    {
        GenericDatagramCreator<Position> genericDatagramCreatorPosition = new GenericDatagramCreator<Position>();
        Position _packet = new Position
        {
            packetType = 1,
            id = Client.instance.id,
            elapsedTime = elaspedTime,
            position = GameManager.players[Client.instance.id].transform.position
        };
        byte[] _pack = genericDatagramCreatorPosition.GetBytes(_packet);
        SendUDPData(_pack);
       
    }

    public static void PlayerRotation(float _rotationX, bool _leftOf)
    {
         GenericDatagramCreator<Rotation> genericDatagramCreatorRotation = new GenericDatagramCreator<Rotation>();
        Rotation _packet = new Rotation
        {
            packetType = 2,
            id = Client.instance.id,
            rotX = _rotationX,
            leftOf = _leftOf
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
    }
    public static void Ping(int _ackNumber)
    {
        GenericDatagramCreator<Ping> genericDatagramCreatorRotation = new GenericDatagramCreator<Ping>();
        Ping _packet = new Ping
        {
            packetType = 3,
            id = Client.instance.id,
            ackNumber = _ackNumber
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
    }
    public static void Ack(int _ackNumber)
    {
        GenericDatagramCreator<Ack> genericDatagramCreatorRotation = new GenericDatagramCreator<Ack>();
        Ack _packet = new Ack
        {
            packetType = 4,
            id = Client.instance.id,
            ackNumber = _ackNumber
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
        DatagramSend.AddToPacketsDictionary(ack, _pack, 2);
        ack++;
    }
    public static void Jump()
    {
        GenericDatagramCreator<Jump> genericDatagramCreatorRotation = new GenericDatagramCreator<Jump>();
        Jump _packet = new Jump
        {
            packetType = 5,
            id = Client.instance.id
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
    }
    public static void Shoot()
    {
        GenericDatagramCreator<Shoot> genericDatagramCreatorRotation = new GenericDatagramCreator<Shoot>();
        Shoot _packet = new Shoot
        {
            packetType = 6,
            id = Client.instance.id
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
    }
    public static void Disconnect()
    {
        GenericDatagramCreator<Disconnect> genericDatagramCreatorRotation = new GenericDatagramCreator<Disconnect>();
        Disconnect _packet = new Disconnect
        {
            packetType = 7,
            id = Client.instance.id
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        SendUDPData(_pack);
    }
    public static void DisconnectReceived(int _ackNumber)
    {
        GenericDatagramCreator<Disconnect> genericDatagramCreatorRotation = new GenericDatagramCreator<Disconnect>();
        Disconnect _packet = new Disconnect
        {
            packetType = 7,
            id = Client.instance.id,
            ackNumber = _ackNumber
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        DatagramSend.AddToPacketsDictionary(ack, _pack, 3);
        SendUDPData(_pack);
        ack++;
    }
    public static void InitialPacket()
    {
        GenericDatagramCreator<InitialPacket> genericDatagramCreatorRotation = new GenericDatagramCreator<InitialPacket>();
        InitialPacket _packet = new InitialPacket
        {

            ackNumber = ack
        };
        byte[] _pack = genericDatagramCreatorRotation.GetBytes(_packet);
        DatagramSend.AddToPacketsDictionary(ack, _pack, 5);
        SendUDPData(_pack);
        ack++;
    }



}


