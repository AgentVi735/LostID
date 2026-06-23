using UnityEngine;

public class PoofParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] systems;

    public void Play()
    {
        foreach (ParticleSystem system in systems)
            system.Play();
    }
}
