using UnityEngine;

public class ParticalRestrictor : MonoBehaviour
{
    [Tooltip("Allow particles to leak out of the cylinder slowly.")]
    public bool allowLeak = false;

    [Tooltip("Chance (0 to 1) that a particle leaks when exiting.")]
    [Range(0f, 1f)]
    public float leakChance = 0.1f;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Particle")) // Make sure your particles use this tag
        {
            if (allowLeak)
            {
                float rand = Random.value;
                if (rand < leakChance)
                {
                    // Let it go
                    return;
                }
            }

            Destroy(other.gameObject); // Contain it (destroy or teleport it back in)
        }
    }
}
