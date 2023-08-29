using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketDirtGrab : MonoBehaviour
{
    public static BucketDirtGrab Instance {  get; private set; }
    public event EventHandler OnGrab;
    public new BoxCollider collider;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            Debug.Log("success");
            OnGrab?.Invoke(this, EventArgs.Empty);
        }
    }
}
