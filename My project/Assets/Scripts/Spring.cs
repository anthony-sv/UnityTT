using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring
{
    public Mass particle_1, particle_2;

    public float dampingFactor;
    public float stiffness;
    public float restLength;

    public bool isDeformed = false;
    public bool isDeformedRecently = false;
    public float maxStress = 0.25f;

    public Spring(Mass _particle_1, Mass _particle_2, float _dampingFactor, float _stiffness)
    {
        particle_1 = _particle_1;
        particle_2 = _particle_2;
        dampingFactor = _dampingFactor;
        stiffness = _stiffness;
        restLength = getLenght();
    }

    private float getLenght()
    {
        return Vector3.Distance(particle_1.GetPosition(), particle_2.GetPosition());
    }

    public Vector3 _CalculateForce()
    {
        Vector3 PA = particle_1.GetPosition();
        Vector3 VA = particle_1.GetVelocity();

        Vector3 PB = particle_2.GetPosition();
        Vector3 VB = particle_2.GetVelocity();

        Vector3 PAB = PB - PA;
        Vector3 VAB = VB - VA;

        float length = Vector3.Distance(PA, PB);

        float linearTerm = length - restLength;
        float nonLinearTerm = restLength * restLength / length - restLength;

        float Fs = stiffness * (linearTerm - nonLinearTerm);
        float Fd = Vector3.Dot(PAB.normalized, VAB) * dampingFactor;

        float Ft = Fs + Fd;

        Vector3 F = PAB.normalized * Ft;
        return F;
    }

    public Vector3 CalculateForces()
    {
        if (isDeformed)
        {
            isDeformedRecently = false;
            return new Vector3(0,0,0);
        }

        float stress = Mathf.Abs(getLenght() - restLength) / restLength;

        if (stress > maxStress)
        {
            isDeformed = true;
            isDeformedRecently = true;
            return new Vector3(0,0,0);
        }

        Vector3 FA = _CalculateForce();
        return FA;
    }
};
