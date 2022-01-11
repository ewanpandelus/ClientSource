using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> processOnMainThread = new List<Action>();
    private static readonly List<Action> processCopiedOnMainThread = new List<Action>();
    private static bool actionToProcessOnMainThread = false;

    private void Update()
    {
        UpdateMain();
    }


    public static void ProcessOnMainThread(Action _action)
    {
        if (_action == null)
        {
            return;
        }

        lock (processOnMainThread)
        {
            processOnMainThread.Add(_action);
            actionToProcessOnMainThread = true;
        }
    }

  
    public static void UpdateMain()
    {
        if (actionToProcessOnMainThread)
        {
            processCopiedOnMainThread.Clear();
            lock (processOnMainThread)
            {
                processCopiedOnMainThread.AddRange(processOnMainThread);
                processOnMainThread.Clear();
                actionToProcessOnMainThread = false;
            }

            for (int i = 0; i < processCopiedOnMainThread.Count; i++)
            {
                processCopiedOnMainThread[i]();
            }
        }
    }
}