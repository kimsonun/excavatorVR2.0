using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public GameObject target;

    private void Update()
    {
        transform.position = target.transform.position;
        //if (gameObject.layer == 5)
            transform.rotation = target.transform.rotation;
    }
}

