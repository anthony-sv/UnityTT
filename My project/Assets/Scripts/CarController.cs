using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarController : MonoBehaviour
{
    public Vector3 initialForce;

    [SerializeField] private GameObject softBody;
    [SerializeField] private GameObject FrontLeftWheel;
    [SerializeField] private GameObject FrontRightWheel;
    [SerializeField] private GameObject RearLeftWheel;
    [SerializeField] private GameObject RearRightWheel;

    private MassSpringSystem MSSystem;

    private Mesh[] MeshWheels = new Mesh[4];
    private Vector3[][] originalVerticesWheels = new Vector3[4][];
    private Rigidbody[] RBWheels = new Rigidbody[4];
    /*
        RBWHeels[0] = FrontLeft
        RBWHeels[1] = FrontRight
        RBWHeels[2] = RearLeft
        RBWHeels[3] = RearRight
    */

    // Start is called before the first frame update
    void Start()
    {
        MSSystem = softBody.GetComponent<MassSpringSystem>();
        RBWheels[0] = FrontLeftWheel.GetComponent<Rigidbody>();
        RBWheels[1] = FrontRightWheel.GetComponent<Rigidbody>();
        RBWheels[2] = RearLeftWheel.GetComponent<Rigidbody>();
        RBWheels[3] = RearRightWheel.GetComponent<Rigidbody>();

        MeshWheels[0] = FrontLeftWheel.GetComponent<MeshFilter>().sharedMesh;
        MeshWheels[1] = FrontRightWheel.GetComponent<MeshFilter>().sharedMesh;
        MeshWheels[2] = RearLeftWheel.GetComponent<MeshFilter>().sharedMesh;
        MeshWheels[3] = RearRightWheel.GetComponent<MeshFilter>().sharedMesh;
        
        for(int i = 0; i < 4; i++)
		{
            originalVerticesWheels[i] = MeshWheels[i].vertices;
        }

        AttachWheels();
    }

    public void SetAngle(float angle)
	{
        Quaternion quat = Quaternion.Euler(0f, angle, 0f);
        transform.rotation = quat;
	}

    public void SetVelocity(float velocity)
	{
        MSSystem.SetVelocity(transform.forward*velocity);
	}


    public float maxForce;
    void AttachWheels()
	{
        int joints = 100;
        foreach(Rigidbody rb in RBWheels)
		{
            List<Mass> particles = MSSystem.particles.ToList();

            particles = particles.OrderBy((b) =>
            {
                float dist = Vector3.Distance(rb.position, b.GetPosition());
                return dist;
            }).ToList();

            for (int j = 0; j < joints; j++)
            {

                Mass particle = particles[j];
                FixedJoint joint = particle.particleObject.AddComponent<FixedJoint>();
                joint.connectedBody = rb;
                joint.breakForce = maxForce;
            }
        }
	}
	private void OnApplicationQuit()
	{
        MSSystem.RestoreOriginalMesh();
        RestoreWheelsMesh();
    }

    private void RestoreWheelsMesh()
	{
        for(int i = 0; i < 4; i++)
		{
            MeshWheels[i].vertices = originalVerticesWheels[i];
            MeshWheels[i].RecalculateNormals();
            MeshWheels[i].RecalculateBounds();
        }
	}

}
