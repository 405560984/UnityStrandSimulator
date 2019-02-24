using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsLab
{
	public class RopeSolver : MonoBehaviour {
		public Transform ParticlePrefab;
		public int Count = 3;
		public int Space = 1;
		[Range(0, 1)]
		public float Damping = 0.1f;
		public Vector3 ExternForce = Vector3.zero;
		public int SolveCount = 1;

		[Header("Collider")]
		public RopeSphereCollider sphereCollider;

		private List<Transform> chain = new List<Transform>();
		private List<Particle> particleList = new List<Particle>();

		void Start()
		{
			for (int i=0; i<Count; i++)
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

		void FixedUpdate ()
		{
            float dt = Time.fixedDeltaTime;

            // Update Velocity
            for (int i = 1; i < Count; i++)
            {
                var particle = particleList[i];

                // Time Integration
                Vector3 vel = particle.velocity + ExternForce * dt;
                particle.prevPos = particle.pos;
                particle.pos = particle.pos + (1 - Damping) * vel * dt;
            }

            // Resolve Constraints
            // 2. Keep Length Constraint from top to bottom
            for (int n = 0; n < SolveCount; n++)
            {
                // Distance Constraint
                for (int i = 1; i < Count; i++)
                {
                    Vector3 offsetToParent = particleList[i].pos - particleList[i - 1].pos;
                    // Strategy 1: only move child particle
                    //particleList[i].pos = particleList[i-1].pos + Space * offsetToParent.normalized;
                    // Strategy 2: Position Based Dynamics, iteratively
                    offsetToParent = Space * offsetToParent.normalized - offsetToParent;
                    particleList[i - 1].pos -=  0.5f * offsetToParent;
                    particleList[i].pos += 0.5f * offsetToParent;
                }

                //for (int i = 2; i < Count; i += 2)
                //{
                //    Vector3 offsetToParent = particleList[i].pos - particleList[i - 2].pos;
                //    // Strategy 1: only move child particle
                //    //particleList[i].pos = particleList[i-1].pos + Space * offsetToParent.normalized;
                //    // Strategy 2: Position Based Dynamics, iteratively
                //    offsetToParent = 2 * Space * offsetToParent.normalized - offsetToParent;
                //    if (offsetToParent.magnitude < 0)
                //    {
                //        particleList[i - 2].pos -= 0.5f * offsetToParent;
                //        particleList[i].pos += 0.5f * offsetToParent;
                //    }
                //}

                // Bend Constraint
                for (int i=1; i<Count-1; i++)
                {
                    Particle lastP = particleList[i - 1];
                    Particle cP = particleList[i];
                    Particle nextP = particleList[i + 1];

                    //Vector3 
                }

                // Attach Root Particle to base transform
                particleList[0].pos = transform.position;
            }

            // 1. Attach Root Particle to base transform
            particleList[0].pos = transform.position;

            // Collision Detection & Response
            if (sphereCollider != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    sphereCollider.UpdateCollision(particleList[i]);
                }
            }

            // Update velocity
            particleList[0].velocity = Vector3.zero;
            for (int i = 1; i < Count; i++)
            {
                particleList[i].velocity = (particleList[i].pos - particleList[i].prevPos) / dt;
            }

            // Apply Particle Data to Transform
            for (int i = 0; i < Count; i++)
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

	public class Particle
	{
        public float invMass;
		public float radius;
		public Vector3 pos;
		public Vector3 prevPos;
		public Vector3 velocity;
	}
}
