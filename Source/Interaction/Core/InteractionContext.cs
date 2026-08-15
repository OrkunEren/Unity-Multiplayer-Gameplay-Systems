using UnityEngine;

namespace InvadersOverboard.Interaction
{
    public readonly struct InteractionContext
    {
        public GameObject InteractorObject
        {
            get;
        }

        public Transform Origin
        {
            get;
        }

        public Collider TargetCollider
        {
            get;
        }

        public Vector3 HitPoint
        {
            get;
        }

        public Vector3 HitNormal
        {
            get;
        }

        public float Distance
        {
            get;
        }


        public InteractionContext(
            GameObject interactorObject,
            Transform origin,
            Collider targetCollider,
            Vector3 hitPoint,
            Vector3 hitNormal,
            float distance)
        {
            InteractorObject =
                interactorObject;

            Origin =
                origin;

            TargetCollider =
                targetCollider;

            HitPoint =
                hitPoint;

            HitNormal =
                hitNormal;

            Distance =
                distance;
        }
    }
}