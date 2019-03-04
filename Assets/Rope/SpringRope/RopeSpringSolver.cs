using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsLab
{
    public class RopeSpringSolver : MonoBehaviour
    {
        public Transform ParticlePrefab;
        public int SubStepCount = 10;
        public int Count = 3;
        public int Space = 1;
        public float SpringK = 1.0f;
        public float AirResistanceRatio = 0.1f;
        [Range(0, 1)]
        public float Damping = 0.1f;

        public Vector3 ExternForce = Vector3.zero;

        private List<Transform> chain = new List<Transform>();
        private List<SpringParticle> particleList = new List<SpringParticle>();

        void Start()
        {
            for (int i = 0; i < Count; i++)
            {
                var obj = Instantiate(ParticlePrefab, transform, true);
                obj.Translate(0, -i * Space, 0);
                chain.Add(obj);

                // Construct Particles
                var particle = new SpringParticle();
                particle.invMass = 1;
                particle.radius = 0.5f * Space;
                particle.pos = new Vector3(0, -i * Space, 0);
                particle.velocity = Vector3.zero;
                particleList.Add(particle);
            }
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime / SubStepCount;

            // Update Particle Position
            // Root Particle follow Transform
            particleList[0].pos = transform.position;
            for (int n = 0; n<SubStepCount; n++)
            {
                for (int i = 1; i < Count; i++)
                {
                    var particle = particleList[i];

                    // Calculate Spring Force
                    // Last Particle
                    Vector3 forceDir = particleList[i - 1].pos - particle.pos;
                    Vector3 springForce = SpringK * forceDir.normalized * (forceDir.magnitude - Space);

                    // Next Particle
                    if (i < Count - 1)
                    {
                        forceDir = particleList[i + 1].pos - particle.pos;
                        springForce += SpringK * forceDir.normalized * (forceDir.magnitude - Space);
                    }

                    // Update Particle Position according to Newton's 2nd Law
                    particle.pos += (1 - Damping) * particle.velocity * dt;

                    // Update velocity
                    Vector3 acceleration = (springForce + ExternForce - AirResistanceRatio * particle.velocity.magnitude * particle.velocity) * particle.invMass;
                    particle.velocity += acceleration * dt;
                }
            }

            // Apply Particle Position to Transform
            for (int i=0; i<Count; i++)
            {
                chain[i].position = particleList[i].pos;
            }
        }

        void OnDrawGizmos()
        {
            if (particleList == null || particleList.Count != Count) return;

            Gizmos.color = Color.blue;
            for (int i = 1; i < Count; i++)
            {
                var particleParent = particleList[i - 1];
                var particle = particleList[i];
                Debug.DrawLine(particleParent.pos, particle.pos);
            }
        }
    }

    public class SpringParticle
    {
        public float invMass; // 1 / mass
        public float radius;
        public Vector3 pos;
        public Vector3 velocity;
    }
}