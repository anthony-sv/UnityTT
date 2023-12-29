using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    [SerializeField] private GameObject lookAtObject;
    public float velocity = 5.0f;
    public Vector3 dist;
    
   
    void FixedUpdate()
    {
        Vector3 dPos = lookAtObject.transform.position + dist;
        Vector3 sPos = Vector3.Lerp(transform.position, dPos, velocity * Time.fixedDeltaTime);
        transform.position = sPos;
        transform.LookAt(lookAtObject.transform);
    }
}
