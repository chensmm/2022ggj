using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwitchDoor : MonoBehaviour
{
    public GameObject Door;
    public Vector3 MovePath;

    Vector3 startPos;

    private void Start()
    {
        startPos = Door.transform.position;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Door.transform.DOMove(startPos + MovePath, 1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Door.transform.DOMove(startPos, 1);
    }
}
