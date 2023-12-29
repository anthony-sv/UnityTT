using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodesJSON : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public int id;
        public float x;
        public float y;
        public float z;

        public Node(int _id, float _x, float _y, float _z)
		{
            id = _id;
            x = _x;
            y = _y;
            z = _z;
		}
    }

    [System.Serializable]
    public class Beam
    {
        public int u;
        public int v;

        public Beam(int _u, int _v)
        {
            u = _u;
            v = _v;
        }
    }

    [System.Serializable]
    public class DataContainer
    {
        public List<Node> nodes;
        public List<Beam> beams;
    }

    public TextAsset inputJson;

    public DataContainer Read()
	{
        DataContainer container = JsonUtility.FromJson<DataContainer>(inputJson.text);

        return container;
    }

    public void Write(List<Mass> particles, Transform transformP, List<Spring> springs)
	{
        DataContainer container = new DataContainer();
		List<Node> nodes = new List<Node>();
        List<Beam> beams = new List<Beam>();

        foreach (Mass particle in particles)
		{
            Vector3 position = particle.GetPosition();
            position = transformP.InverseTransformPoint(position);
            Node n = new Node(particle.ID, position.x, position.y, position.z);
            nodes.Add(n);
		}

        foreach(Spring spring in springs)
		{
            int id_1 = spring.particle_1.ID;
            int id_2 = spring.particle_2.ID;
            Beam beam = new Beam(id_1, id_2);
            beams.Add(beam);
        }

        container.nodes = nodes;
        container.beams = beams;

        string jsonText = JsonUtility.ToJson(container, true);
        System.IO.File.WriteAllText(Application.dataPath + "/TextFiles/outputNodes.json", jsonText);
	}
}
