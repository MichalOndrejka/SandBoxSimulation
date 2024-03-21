using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidParticleController : MonoBehaviour
{
    void Update()
    {
        // Check if the Y position is less than 0
        if (transform.position.y < 0f)
        {
            // Destroy the GameObject
            Destroy(gameObject);
        }
    }
}
