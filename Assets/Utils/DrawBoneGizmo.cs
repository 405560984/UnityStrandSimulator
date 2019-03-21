using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoCharacter
{
    public class DrawBoneGizmo : MonoBehaviour
    {
        public Transform RootNode;
        public float Radius = 1e-3f;

        private const int MaxDist = 10;
        private Transform[] childNodes;

        private void OnDrawGizmosSelected()
        {
            if (RootNode == null) return;
            if (childNodes == null) PopulateChildren();

            foreach (Transform child in childNodes)
            {
                if (child == RootNode)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(child.position, Radius);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(child.position, Radius * FindRadioToRoot(child));

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(child.position, child.parent.position);
                }
            }
        }

        private float FindRadioToRoot(Transform child)
        {
            int dist = 0;
            Transform parent = child;
            while ((parent != RootNode) && dist < MaxDist)
            {
                dist++;
                parent = parent.parent;
            }

            return (MaxDist - dist)/(float)MaxDist + 0.1f;
        }

        private void PopulateChildren()
        {
            childNodes = RootNode.GetComponentsInChildren<Transform>();
        }
    }

}