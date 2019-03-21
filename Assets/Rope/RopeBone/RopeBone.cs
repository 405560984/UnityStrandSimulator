using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsLab
{
    public class RopeBone : MonoBehaviour
    {
        public Transform RootBone;
        [Tooltip("Local Direction to Child")]
        public Vector3 BoneAxis = new Vector3(-1.0f, 0.0f, 0.0f);

        [Header("Physics")]
        [Range(0, 1)]
        public float Damping = 0.1f;
        public Vector3 ExternForce = Vector3.zero;
        public int SolveCount = 1;

        private List<BoneParticle> boneParticleList = new List<BoneParticle>();

        void Start()
        {
            // Sample from root bone, store into bone particle list
            Debug.Assert(RootBone != null);

            // Add Root Bone
            BoneParticle bp = new BoneParticle(RootBone);
            boneParticleList.Add(bp);

            while (bp.boneTfm.childCount > 0)
            {
                // Add Child Bone to List
                Transform ct = bp.boneTfm.GetChild(0); // Only Consider the first child
                float lengthToParent = Vector3.Distance(bp.boneTfm.position, ct.position);

                bp = new BoneParticle(ct);
                bp.invMass = 1;
                bp.lengthToParent = lengthToParent;
                boneParticleList.Add(bp);
            }
        }

        void LateUpdate()
        {
            // Simulate One Step
            float dt = Time.deltaTime;
            SimulateOneStep(1/60.0f);

            // Apply to Bone Transform
            for (int i=0; i<boneParticleList.Count - 1; i++)
            {
                BoneParticle bp = boneParticleList[i];
                BoneParticle cp = boneParticleList[i + 1];
                Vector3 animVector = bp.boneTfm.TransformDirection(BoneAxis);
                Quaternion rotOffset = Quaternion.FromToRotation(animVector, cp.pos - bp.pos);
                bp.boneTfm.rotation = rotOffset * bp.boneTfm.rotation;
            }
            
        }

        private void SimulateOneStep(float dt)
        {
            // Update Velocity
            for (int i=0; i<boneParticleList.Count; i++)
            {
                var particle = boneParticleList[i];

                if (particle.invMass == 0) continue; // Ignore anchor points

                // Time Integration
                Vector3 vel = particle.velocity + ExternForce * dt;
                particle.prevPos = particle.pos;
                particle.pos = particle.pos + (1 - Damping) * vel * dt;
            }

            // Resolve Constraints
            // Keep Length Constraint from top to bottom
            for (int n = 0; n < SolveCount; n++)
            {
                // Distance Constraint
                for (int i = 1; i < boneParticleList.Count; i++)
                {
                    var parentBp = boneParticleList[i - 1];
                    var currentBp = boneParticleList[i];
                    Vector3 offsetToParent = currentBp.pos - parentBp.pos;
                    // Strategy 1: only move child particle
                    //particleList[i].pos = particleList[i-1].pos + Space * offsetToParent.normalized;
                    // Strategy 2: Position Based Dynamics, iteratively
                    offsetToParent = currentBp.lengthToParent * offsetToParent.normalized - offsetToParent;
                    float invMassSum = parentBp.invMass + currentBp.invMass;
                    parentBp.pos -= parentBp.invMass / invMassSum * offsetToParent;
                    currentBp.pos += currentBp.invMass / invMassSum * offsetToParent;
                }
            }

            // Attach Root Particle to bone transform
            boneParticleList[0].pos = boneParticleList[0].boneTfm.position;

            // Collision Detection & Response

            // Update Velocity
            for (int i = 1; i < boneParticleList.Count; i++)
            {
                boneParticleList[i].velocity = (boneParticleList[i].pos - boneParticleList[i].prevPos) / dt;
            }
        }

        void OnDrawGizmos()
        {
            if (boneParticleList.Count <= 0) return;

            Gizmos.color = Color.blue;
            for (int i = 0; i < boneParticleList.Count; i++)
            {
                //var particleParent = particleList[i - 1];
                var particle = boneParticleList[i];
                //Debug.DrawLine(particleParent.pos, particle.pos);
                Gizmos.DrawSphere(particle.pos, 0.02f);
            }
        }
    }

    public class BoneParticle
    {
        public Transform boneTfm;
        public float lengthToParent; // World-Space Length

        public float invMass;
        public float radius;
        public Vector3 pos;
        public Vector3 prevPos;
        public Vector3 velocity;

        public BoneParticle(Transform boneTransform)
        {
            boneTfm = boneTransform;
            lengthToParent = 0;
            invMass = 0;
            radius = 0;
            prevPos = pos = boneTfm.position;
            velocity = Vector3.zero;
        }
    }
}