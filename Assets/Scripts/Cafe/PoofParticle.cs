using UnityEngine;

public class PoofParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] systems;

    public void Play(float size)
    {
        foreach (ParticleSystem system in systems)
        {
            if (size > 0)
            {
                ParticleSystem.MainModule main = system.main;
                main.startSize = size;
            }
            system.Play();
        }
    }
}
