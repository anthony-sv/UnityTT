using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebuggerScript : MonoBehaviour
{
    public GameObject carObject;

    public List< Tuple<Vector3, Vector3> > lines = new List<Tuple<Vector3, Vector3>>();
	public List<Vector3> verts = new List<Vector3>();

	public void addVector(Vector3 start, Vector3 end)
	{
        lines.Add(new Tuple<Vector3, Vector3>(start, end));
	}

	public void addNode(Vector3 position)
	{
		verts.Add(position);
	}

	private void updateVectors()
	{
		foreach (Tuple<Vector3, Vector3> line in lines)
		{
			Gizmos.color = Color.red; // Color de la flecha

			Vector3 endPoint = line.Item2;

			// Dibuja una línea desde el origen hasta el punto final
			Gizmos.DrawLine(line.Item1, endPoint);
			Gizmos.DrawSphere(line.Item2, 0.01f);
			// Gizmos.DrawSphere(line.Item1, 0.05f);

		}
	}

	private void updateNodes()
	{
		Gizmos.color = Color.red;
		foreach (Vector3 vert in verts)
		{
			Gizmos.DrawSphere(vert, 0.05f);
			// Gizmos.DrawSphere(line.Item1, 0.05f);

		}
	}

	private void FixedUpdate()
	{
		/*
		Rigidbody rgbd = carObject.GetComponent<Rigidbody>();
		Debug.Log(rgbd.velocity);
		*/
	}

	public void clearList()
	{
		lines.Clear();
	}

	void OnDrawGizmos()
    {
		updateNodes();
		updateVectors();
	}
}
