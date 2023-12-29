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

    // Este m�todo se llama durante una colisi�n continua (si tienes Collider continuos habilitados)
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain") return;
        // Debug.Log("Colision detectada de particula - stay: " + collision.gameObject.tag);
    }

    // Este m�todo se llama cuando finaliza una colisi�n
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain") return;
        // Debug.Log("Colision detectada de particula - exit: " + collision.gameObject.tag);
    }
}
