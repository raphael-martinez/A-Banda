using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class ParticlesAnimation : PYAnimation
    {
        public ParticleSystem[] Particles;

        public override void Play()
        {
            base.Play();
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].startDelay = DelayToStart;
                Particles[i].Play();
            }
        }

        public override void Reverse()
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].Stop();
            }
        }

        public override void Stop()
        {
            base.Stop();

            Reverse();
        }
    }
}