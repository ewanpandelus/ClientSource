using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private Vector3 prevMousePos;
    [SerializeField] GameObject firePoint;
    private Vector3 worldPos;
    private bool leftOf = false;
    void Update()
    {
      

        Vector3 mousePos = Input.mousePosition;
        if (Vector3.Distance(prevMousePos, mousePos)<31)
        {
            return;
        }
        mousePos.z = -Camera.main.transform.position.z;
        
        worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        if (worldPos.x < transform.position.x)
        {
            leftOf = false;
        }
        else
        {
            leftOf = true;
        }
        transform.LookAt(worldPos);
        prevMousePos = mousePos;
    }
    public Vector3 GetShotDirection()
    {
        Vector3 firePointPos = firePoint.transform.position;
        firePointPos += firePoint.transform.forward * 2;
        return firePointPos-transform.position;
    }
    public bool GetLeftOf()
    {
        return leftOf;
    }
    public float GetRotation()
    {
        return this.transform.rotation.eulerAngles.x;
    }
}
  



