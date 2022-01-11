using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resend : MonoBehaviour
{
    public void StartResending(int packetNo)
    {
        StartCoroutine(CheckImportantPacketSent(packetNo));
    }
    public void StartResending(int packetNo, int timesToSend)
    {
        StartCoroutine(SendFixedTimes(packetNo, timesToSend));
    }
   
    public IEnumerator CheckImportantPacketSent(int packetNo)
    {
        float elapsedTime = 0f;
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (DatagramSend.sentPackets[packetNo] != true &&elapsedTime<15)
            {
                elapsedTime += Time.deltaTime;
                yield return new WaitForSeconds(3f);
                ClientSend.SendUDPData(DatagramSend.resendPacketsContent[packetNo]);
              
         
            }
            else
            {
                Debug.Log("Stopped" + packetNo.ToString());
                DatagramSend.resendPacketsContent.Remove(packetNo);
                yield break;
            }
           
        }
    }
    public IEnumerator SendFixedTimes(int packetNo, int timesToSend)
    {
        for(int i = 0; i<timesToSend; i++)
        {
            if (DatagramSend.sentPackets[packetNo] != true)
            {
                yield return new WaitForSeconds(3f);
                ClientSend.SendUDPData(DatagramSend.resendPacketsContent[packetNo]);
            }
            else
            {
                Debug.Log("Stopped");
                yield break;
            }
        }
        DatagramSend.resendPacketsContent.Remove(packetNo);
    }
   
}
