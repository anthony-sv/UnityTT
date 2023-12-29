using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
        if (collision.gameObject.tag == "Terrain") return;
		// Debug.Log("Colision detectada de particula: " + collision.gameObject.tag);
	}

    // Este método se llama durante una colisión continua (si tienes Collider continuos habilitados)
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain") return;
        // Debug.Log("Colision detectada de particula - stay: " + collision.gameObject.tag);
    }

    // Este método se llama cuando finaliza una colisión
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain") return;
        // Debug.Log("Colision detectada de particula - exit: " + collision.gameObject.tag);
    }
}
