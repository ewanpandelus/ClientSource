using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        Destroy(gameObject, 0.05f);
    }
}
