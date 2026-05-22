using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleWithAttractor : MonoBehaviour
{
    [SerializeField] private ParticleType particleType;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private Transform particleParent;
    [SerializeField] private UIParticleAttractor particleAttractor;

    public void PlayParticleWithAttractor(Transform  particlePosition)
    {
        particleParent.transform.position = particlePosition.position;
        particle.Play();
        Destroy(gameObject,3f);
    }
}

public enum ParticleType
{
    None,
    Coin,
    Gems
}
