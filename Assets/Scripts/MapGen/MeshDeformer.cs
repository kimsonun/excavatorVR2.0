using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    public float radius = 2f;
    public float deformationStrength = 2f;
    private Mesh mesh;
    private Vector3[] verticies, modifiedVerts;
    private float range = 0.17f;
    private void Start()
    {
        mesh = GetComponentInChildren<MeshFilter>().mesh;
        verticies = mesh.vertices;
        modifiedVerts = mesh.vertices;
        BucketDirtGrab.Instance.OnGrab += BucketDirtGrab_OnGrab;
    }

    private void BucketDirtGrab_OnGrab(object sender, System.EventArgs e)
    {
        Ray ray = new Ray(BucketDirtGrab.Instance.collider.transform.position, BucketDirtGrab.Instance.collider.transform.TransformDirection((Vector3.back - Vector3.down) * range));
        
        if (BucketDirtGrab.Instance.collider.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("1st if");
            for (int v = 0; v < modifiedVerts.Length; v++)
            {
                Vector3 disctance = modifiedVerts[v] - hit.point;

                float smoothingFactor = 2f;
                float force = deformationStrength / (1f + hit.point.sqrMagnitude);

                if (disctance.sqrMagnitude < radius)
                {
                    Debug.Log("2nd if");
                    modifiedVerts[v] = modifiedVerts[v] + (Vector3.down * force) / smoothingFactor;
                }
            }
        }
    }

    void RecalculateMesh()
    {
        mesh.vertices = modifiedVerts;
        GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
    }
    private void Update()
    {
        Debug.DrawRay(BucketDirtGrab.Instance.collider.transform.position, BucketDirtGrab.Instance.collider.transform.TransformDirection((Vector3.back - Vector3.down) * range), Color.green);

        /*
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward * range));
        if (ExcavatorController.Instance.bucketCollider.Raycast(ray, out hit, range)) 
        {
            Debug.Log("1st if");
            for (int v = 0; v < modifiedVerts.Length; v++)
            {
                Vector3 disctance = modifiedVerts[v] - hit.point;

                float smoothingFactor = 2f;
                float force = deformationStrength / (1f + hit.point.sqrMagnitude);

                if (disctance.sqrMagnitude < radius)
                {
                    Debug.Log("2nd if");
                    modifiedVerts[v] = modifiedVerts[v] + (Vector3.down * force) / smoothingFactor;
                }
            }
        }
        */
        RecalculateMesh();
    }

    /*
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("1st if");
        for (int v = 0; v < modifiedVerts.Length; v++)
        {
            Vector3 disctance = modifiedVerts[v] - collision.contacts[0].point;

            float smoothingFactor = 2f;
            float force = deformationStrength / (1f + collision.contacts[0].point.sqrMagnitude);

            if (disctance.sqrMagnitude < radius)
            {
                Debug.Log("2nd if");
                modifiedVerts[v] = modifiedVerts[v] + (Vector3.down * force) / smoothingFactor;
            }
        }
    }
    */
}
