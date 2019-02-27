using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsLab
{
    public class RopeSpringSolver : MonoBehaviour
    {
        public Transform ParticlePrefab;
        public int Count = 3;
        public int Space = 1;
        public float SpringK = 1.0f;

        public Vector3 ExternForce = Vector3.zero;

        [Header("Collider")]
        public RopeSphereCollider sphereCollider;

        private List<Transform> chain = new List<Transform>();
        private List<Particle> particleList = new List<Particle>();

        void Start()
        {
            for (int i = 0; i < Count; i++)
            {
                var obj = Instantiate(ParticlePrefab, transform, true);
                obj.Translate(0, -i * Space, 0);
                chain.Add(obj);

                // Construct Particles
                var particle = new Particle();
                particle.invMass = 1;// i == 0 ? 0 : 1; // 0 means Mass is infinite
                particle.radius = 0.5f * Space;
                particle.pos = particle.prevPos = new Vector3(0, -i * Space, 0);
                particle.velocity = Vector3.zero;
                particleList.Add(particle);
            }
        }

        void FixedUpdate()
        {

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
}