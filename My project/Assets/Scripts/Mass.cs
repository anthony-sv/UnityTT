using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mass
{
    public float particleRadius = 0.009f;

    public int ID;
    public List<Spring> springs;

    private Vector3 position;
    private Vector3 velocity;

    public GameObject particleObject;
    public Rigidbody rb;
    public SphereCollider collider;

    private ParticleCollision PC;

    Mass() { }

    // Cada partícula será un gameObject con propiedades físicas (RigidBody)
    public Mass(int _ID, float _mass, Vector3 _positon, Transform parent, bool gravity = false)
    {
        ID = _ID;
        springs = new List<Spring>();

        particleObject = new GameObject("Particle");
        particleObject.tag = "Particle";
        particleObject.transform.position = _positon;

        // Agregamos un componente rigid body
        rb = particleObject.AddComponent<Rigidbody>();
        rb.mass = _mass;
        rb.useGravity = gravity;
        rb.freezeRotation = true;
        position = rb.position;
        velocity = rb.velocity;

        // Agregamos un collider esférico muy pequeño
        collider = particleObject.AddComponent<SphereCollider>();
        collider.radius = particleRadius;

        // Debugger
        PC = particleObject.AddComponent<ParticleCollision>();

        // Será un objeto dentro del sistema en conjunto
        particleObject.transform.parent = parent;
    }

    public void UpdateRB()
	{
        position = rb.position;
        velocity = rb.velocity;
	}

    public Vector3 GetPosition()
    {
        return position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public void ApplyVelocity(Vector3 velocity)
	{
        rb.velocity = velocity;
	}

    public void ApplyForce(Vector3 force)
    {
        rb.AddForce(force);
    }
};