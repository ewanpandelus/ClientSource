using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject explosion;

    public void Explode( Vector3 _position, bool _win)
    {
        GameObject explode = Instantiate(explosion, _position, Quaternion.identity);
        for (int i = 0; i<explode.transform.childCount; i++)
        {
            GameObject piece = explode.transform.GetChild(i).gameObject;
            float randSize = Random.Range(0.2f, 0.5f);
            Vector3 scale = piece.transform.localScale;
            scale.Set(randSize, randSize, randSize);
            piece.transform.localScale = scale;
            piece.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0, 360), Random.Range(0, 360), 0)/Random.Range(3,10), ForceMode.Impulse);
        }
        GameObject textObj = GameObject.Find("EndGameText");
        if (_win) textObj.GetComponent<TextMeshProUGUI>().text = "Winner!!!";
        else textObj.GetComponent<TextMeshProUGUI>().text = "Unfortunately you have been defeated...";
    }
}
