using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MassSpringSystem : MonoBehaviour {
    // Mesh
    private Mesh carMesh;

    // Vertices
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    private int[] triangles;

    // Variables
    public int particlesCount;
    public Mass[] particles;
    private int springsCount;
    private Spring[] springs;

    // Variables for parallelizing
    private Vector3[] totalForces;

    // Mapping
    private int[] reindex; // reindex for duplicated vertices (vertices with same position but different normals)
    private Dictionary<int, int> idx_mesh_to_particle = new Dictionary<int, int>();
    private Dictionary<int, List<int>> repr_to_copies = new Dictionary<int, List<int>>();
    private List<int> reprs = new List<int>();
    HashSet<string> uniqueSprings = new HashSet<string>();

    // Files to read/write Particles data
    public NodesJSON jsonHandler;

    // Hyperparametros
    public float MASS = 2f;
    public bool gravity = false;
    public float DAMPING = 30f;
    public float STIFFNESS = 1000f;

    // DEBUGGER
    public GameObject debObject;
    public DebuggerScript debugger;

    void Start()
    {
        if(debObject != null)
            debugger = debObject.GetComponent<DebuggerScript>();
        // Obtiene el mesh filter, el mesh collider y las posiciones de los vertices del mesh, así como los triángulos
        carMesh = GetComponent<MeshFilter>().sharedMesh;
        // GetComponent<MeshRenderer>().enabled = false;

        originalVertices = carMesh.vertices;
        modifiedVertices = carMesh.vertices;
        triangles = carMesh.triangles;


        CalculateReindexation();
        CreateParticlesByFile();
        CreateSpringsByFile();
        CreateSpringsMeshToBase();
        // CreateParticles();
        // CreateSpringsByTriangles();
        IgnoreCollisionBetweenParticles();
        totalForces = new Vector3[particlesCount];

        // WriteJSONMeshAndSprings();
        Debug.Log("Springs: " + springs.Length);
        Debug.Log("Vertices en Mesh: " + originalVertices.Length + "\n" + "Particulas: " + particlesCount);
        Debug.Log("Unique vertices: " + repr_to_copies.Count);

        Physics.gravity = new Vector3(0, -9.81f * 2, 0);
        Time.timeScale = 0.5f;
        Debug.Log(Time.fixedDeltaTime);
    }

    public void ApplyForce(Vector3 force)
	{
        foreach (Mass particle in particles)
        {
            particle.ApplyForce(force);
        }
    }

    public void SetVelocity(Vector3 velocity)
	{
        foreach (Mass particle in particles)
        {
            particle.ApplyVelocity(velocity);
        }
    }

	private void FixedUpdate()
    {
        UpdateParticlesRB();
        CalculateAllForces();
        ApplyAllForces();
    }

	private void LateUpdate()
	{
        UpdateVertices();
    }

	private void IgnoreCollisionBetweenParticles()
	{
        for(int i = 0; i < particlesCount; i++)
		{
            Mass particle_1 = particles[i];
            for (int j = i+1; j < particlesCount; j++)
			{
                Mass particle_2 = particles[j];
                Physics.IgnoreCollision(particle_1.collider, particle_2.collider);
            }
		}
    }

    private void UpdateParticlesRB()
	{
        foreach (Mass particle in particles)
        {
            particle.UpdateRB();
        }
    }

    private void CalculateAllForces()
    {
        ConcurrentBag<int> springs_deformed = new ConcurrentBag<int>();
        Array.Clear(totalForces, 0, totalForces.Length);
        Parallel.For(0, springs.Length, i =>
        {
            Spring spring = springs[i];
            Mass part_1 = spring.particle_1;
            Mass part_2 = spring.particle_2;

            Vector3 force_1 = spring.CalculateForces();
            totalForces[part_1.ID] += force_1;
            totalForces[part_2.ID] -= force_1;

            if(spring.isDeformedRecently)
			{
                springs_deformed.Add(i);
			}
        });

        foreach(int idx_spring in springs_deformed)
		{
            Spring spring = springs[idx_spring];
            Mass part_1 = spring.particle_1;
            Mass part_2 = spring.particle_2;
            FixedJoint joint = part_1.particleObject.AddComponent<FixedJoint>();
            joint.connectedBody = part_2.rb;
        }
    }

    private void ApplyAllForces()
	{
        for (int i = 0; i < particlesCount; i++)
        {
            particles[i].ApplyForce(totalForces[i]);
        }
    }

    private void UpdateVertices()
    {
        UpdateParticlesRB();
        for (int i = 0; i < reprs.Count; i++)
        {
            int idxMesh = reprs[i];
            foreach(int idxSame in repr_to_copies[idxMesh])
			{
                modifiedVertices[idxSame] = transform.InverseTransformPoint(particles[i].GetPosition());
            }
        }
        
        carMesh.vertices = modifiedVertices;
        carMesh.RecalculateNormals();
        carMesh.RecalculateBounds();
    }

    private void CalculateReindexation()
    {
        Dictionary<Vector3, int> reprPosition = new Dictionary<Vector3, int>();
        reindex = new int[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 position = originalVertices[i];
            if (!reprPosition.ContainsKey(position))
            {
                reprPosition[position] = i;
                reprs.Add(i);
            }

            int repr = reprPosition[position];
            reindex[i] = repr;

            if (!repr_to_copies.ContainsKey(repr))
            {
                repr_to_copies[repr] = new List<int>();
            }
            repr_to_copies[repr].Add(i);
        }
    }

    // To create model
    private void WriteJSONMeshAndSprings()
	{
        jsonHandler.Write(particles.ToList(), transform, springs.ToList());
	}

    // Create particles and springs
    private float GetStiffness(Vector3 pos_1, Vector3 pos_2)
	{
        return STIFFNESS;
	}

    private void CreateParticles()
	{
        // int extraParticles = 63 + 1;
        particlesCount = reprs.Count;
        particles = new Mass[particlesCount];
        for(int i = 0; i < reprs.Count; i++)
        {
            int idxMesh = reprs[i];
            Vector3 position = originalVertices[idxMesh];

            Mass particle = new Mass(i, MASS, transform.TransformPoint(position), transform, gravity);
            particles[i] = particle;
            idx_mesh_to_particle[idxMesh] = i;
        }

        /*
        int countExtra = 0;
        for (float i = -1; i <= 1; i += 0.4f)
        {
            for (float j = -1; j <= 1; j += 0.4f)
            {
                for (float k = -1; k <= 1; k += 0.4f)
                {
                    if (Mathf.Abs(i) == 1 || Mathf.Abs(j) == 1 || Mathf.Abs(k) == 1) continue;

                    Vector3 position = new Vector3(i, j, k);
                    // Debug.Log(countExtra);
                    particles[reprs.Count + countExtra] = new Mass(particlesCount, MASS, transform.TransformPoint(position), transform, gravity);
                    particlesCount++;
                    countExtra++;
                }
            }
        }
        */
        
    }

    private void CreateParticlesByFile()
	{
        NodesJSON.DataContainer container =  jsonHandler.Read();
        List<NodesJSON.Node> nodes = container.nodes;

        particlesCount = nodes.Count;

        particles = new Mass[particlesCount];
        foreach(NodesJSON.Node node in nodes)
		{
            int idx = node.id;
            Vector3 position = new Vector3(node.x, node.y, node.z);
            particles[idx] = new Mass(idx, MASS, transform.TransformPoint(position), transform, gravity);
		}
	}

    private void CreateSpringsByFile()
    {
        NodesJSON.DataContainer container = jsonHandler.Read();
        List<NodesJSON.Beam> beams = container.beams;

        springsCount = 0;
        springs = new Spring[beams.Count];
        for (int i = 0; i < beams.Count; i++)
		{
            NodesJSON.Beam beam = beams[i];
            int id_u = beam.u;
            int id_v = beam.v;

            // Para asegurar que no tenga resortes repetidos
            int[] id_springs = { id_u, id_v };
            Array.Sort(id_springs);
            string cadena_id = $"{id_springs[0]} - {id_springs[1]}";
            if (uniqueSprings.Contains(cadena_id)) continue;
            uniqueSprings.Add(cadena_id);

            springs[springsCount++] = new Spring(particles[id_u], particles[id_v], DAMPING, STIFFNESS);
		}
		Debug.Log("Beams: " + beams.Count);
        Debug.Log("Springs: " + springsCount);

        // Redimensiona el arreglo al tamaño real
        System.Array.Resize(ref springs, springsCount);
    }

    private string GetHash(Mass part1, Mass part2) {
        int idU = part1.ID;
        int idV = part2.ID;
        int[] id_springs = {idU, idV};
        Array.Sort(id_springs);
        string cadenaID = $"{id_springs[0]} - {id_springs[1]}";

        return cadenaID;
    }

    private void CreateSpringsMeshToBase()
	{
        int cntMeshToBase = 15;
        
        System.Array.Resize(ref springs, springsCount + cntMeshToBase * reprs.Count);
        // Debug.Log(springs.Length);
        for (int i = 0; i < reprs.Count; i++)
		{
            List<Mass> particlesBase = new List<Mass>();
            Mass particleMesh = particles[i];

            for (int j = reprs.Count; j < particlesCount; j++)
			{
                Mass particleBase = particles[j];


                string cadenaID = GetHash(particleMesh, particleBase);
                if (uniqueSprings.Contains(cadenaID)) continue;

                particlesBase.Add(particleBase);
            }

            particlesBase = particlesBase.OrderBy((b) =>
            {
                float dist = Vector3.Distance(particleMesh.GetPosition(), b.GetPosition());
                return dist;
            }).ToList();

            for(int j = 0; j < cntMeshToBase; j++)
			{
                springs[springsCount++] = new Spring(particleMesh, particlesBase[j], DAMPING, STIFFNESS);
                string cadenaID = GetHash(particleMesh, particlesBase[j]);
                uniqueSprings.Add(cadenaID);
            }
		}
	}

    private float maxDistanceSprings = 0.4f;
    private void CreateSpringsByDistance()
	{
        HashSet<string> uniqueSprings = new HashSet<string>();
        int total_springs = 0;

        int n = particles.Length * particles.Length;
        n += triangles.Length;

        springs = new Spring[n];
        for (int i = 0; i < particles.Length; i++)
        {
            for (int j = i+1; j < particles.Length; j++)
            {
                int id_particle_curr = i;
                int id_particle_next = j;

                // Para asegurar que no tenga resortes repetidos
                int[] id_springs = { id_particle_curr, id_particle_next };
                Array.Sort(id_springs);
                string cadena_id = $"{id_springs[0]} {id_springs[1]}";
                if (uniqueSprings.Contains(cadena_id)) continue;
                uniqueSprings.Add(cadena_id);


                Mass particle_1 = particles[id_particle_curr];
                Mass particle_2 = particles[id_particle_next];

                float distance = Vector3.Distance(particle_1.GetPosition(), particle_2.GetPosition());
                if (distance > maxDistanceSprings)
                {
                    continue;
				}

                Spring spring = new Spring(particle_1, particle_2, DAMPING, GetStiffness(particle_1.GetPosition(), particle_2.GetPosition()));
                springs[total_springs++] = spring;

                // Agregar cada referencia del resorte a su respectivas masas
                particle_1.springs.Add(spring);
                particle_2.springs.Add(spring);
            }
        }


        // Springs between triangles
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int[] triangle = { reindex[triangles[i]], reindex[triangles[i + 1]], reindex[triangles[i + 2]] };
            for (int j = 0; j < 3; j++)
            {
                if (!idx_mesh_to_particle.ContainsKey(triangle[j])) continue;
                if (!idx_mesh_to_particle.ContainsKey(triangle[(j + 1) % 3])) continue;

                int id_particle_curr = idx_mesh_to_particle[triangle[j]];
                int id_particle_next = idx_mesh_to_particle[triangle[(j + 1) % 3]];

                // Para asegurar que no tenga resortes repetidos
                int[] id_springs = { id_particle_curr, id_particle_next };
                Array.Sort(id_springs);
                string cadena_id = $"{id_springs[0]} {id_springs[1]}";
                if (uniqueSprings.Contains(cadena_id)) continue;
                uniqueSprings.Add(cadena_id);

                Mass particle_1 = particles[id_particle_curr];
                Mass particle_2 = particles[id_particle_next];

                // Crear un resorte por cada arista en el triangulo
                Spring spring = new Spring(particle_1, particle_2, DAMPING, STIFFNESS);
                springs[total_springs++] = spring;

                // Agregar cada referencia del resorte a su respectivas masas
                particle_1.springs.Add(spring);
                particle_2.springs.Add(spring);
            }
        }

        // Redimensiona el arreglo al tamaño real
        System.Array.Resize(ref springs, total_springs);
    }

    private void CreateSpringsByTriangles()
    {
        HashSet<string> uniqueSprings = new HashSet<string>();
        // Crear los resortes basados en los triángulos del mesh
        int total_springs = 0;
        springs = new Spring[triangles.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int[] triangle = { reindex[triangles[i]], reindex[triangles[i + 1]], reindex[triangles[i + 2]] };
            for (int j = 0; j < 3; j++)
            {
                int id_particle_curr = idx_mesh_to_particle[triangle[j]];
                int id_particle_next = idx_mesh_to_particle[triangle[(j + 1) % 3]];

                // Para asegurar que no tenga resortes repetidos
                int[] id_springs = { id_particle_curr, id_particle_next };
                Array.Sort(id_springs);
                string cadena_id = $"{id_springs[0]} {id_springs[1]}";
                if (uniqueSprings.Contains(cadena_id)) continue;
                uniqueSprings.Add(cadena_id);

                Mass particle_1 = particles[id_particle_curr];
                Mass particle_2 = particles[id_particle_next];

                // Crear un resorte por cada arista en el triangulo
                Spring spring = new Spring(particle_1, particle_2, DAMPING, STIFFNESS);
                springs[total_springs++] = spring;

                // Agregar cada referencia del resorte a su respectivas masas
                particle_1.springs.Add(spring);
                particle_2.springs.Add(spring);
            }
        }
       
        // Redimensiona el arreglo al tamaño real
        System.Array.Resize(ref springs, total_springs);
    }

    // DEBUGG
    private void DebuggMinDist()
    {
        float min_dist = 100;
        for(int i = 0; i < reprs.Count; i++)
		{
            for (int j = i + 1; j < reprs.Count; j++)
            {
                int x = reprs[i];
                int y = reprs[j];
                min_dist = MathF.Min(min_dist, Vector3.Distance(originalVertices[x], originalVertices[y]));
            }
        }

        Debug.Log(min_dist);
    }

    private void DebuggMasses(int idx)
	{
        debugger.addNode(particles[idx].GetPosition());
	}

    private void DebuggNodes(int idx)
    {
        Vector3 target = transform.InverseTransformPoint(particles[idx].GetPosition());
        foreach (Spring spring in springs)
        {
            Vector3 position_1_or = spring.particle_1.GetPosition();
            Vector3 position_2_or = spring.particle_2.GetPosition();

            Vector3 position_1 = transform.InverseTransformPoint(position_1_or);
            Vector3 position_2 = transform.InverseTransformPoint(position_2_or);
            if (position_1 == target)
            {
                debugger.addVector(position_1_or, position_2_or);
            }
            if (position_2 == target)
            {
                debugger.addVector(position_1_or, position_2_or);
            }
        }
    }

    private void DebuggSprings(int idx)
    {
        Vector3 target = transform.InverseTransformPoint(particles[idx].GetPosition());
        foreach (Spring spring in springs)
        {
            Vector3 position_1_or = spring.particle_1.GetPosition();
            Vector3 position_2_or = spring.particle_2.GetPosition();

            Vector3 position_1 = transform.InverseTransformPoint(position_1_or);
            Vector3 position_2 = transform.InverseTransformPoint(position_2_or);
            if (position_1 == target)
            {
                debugger.addVector(position_1_or, position_2_or);
            }
            if (position_2 == target)
            {
                debugger.addVector(position_1_or, position_2_or);
            }
        }
    }

    // RESTORE
    private void OnApplicationQuit()
    {
        // Restaura los vertices del mesh original, es como unn backup
        RestoreOriginalMesh();
    }
    public void RestoreOriginalMesh()
    {
        carMesh.vertices = originalVertices;
        carMesh.RecalculateNormals();
        carMesh.RecalculateBounds();
    }

}
