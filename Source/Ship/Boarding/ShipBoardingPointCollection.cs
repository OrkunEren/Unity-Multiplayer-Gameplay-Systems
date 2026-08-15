using UnityEngine;

namespace InvadersOverboard.Ship.Boarding
{
    [DisallowMultipleComponent]
    public sealed class
        ShipBoardingPointCollection :
            MonoBehaviour
    {
        [SerializeField]
        private ShipBoardingPoint[] points;


        public ShipBoardingPoint[] Points =>
            points;


        public bool TryGetPoint(
            byte pointId,
            out ShipBoardingPoint point)
        {
            if (points != null)
            {
                foreach (ShipBoardingPoint candidate
                         in points)
                {
                    if (candidate == null ||
                        candidate.PointId != pointId)
                    {
                        continue;
                    }

                    point = candidate;
                    return true;
                }
            }

            point = null;
            return false;
        }


        public bool Contains(
            ShipBoardingPoint point)
        {
            if (point == null ||
                points == null)
            {
                return false;
            }

            foreach (ShipBoardingPoint candidate
                     in points)
            {
                if (candidate == point)
                    return true;
            }

            return false;
        }


#if UNITY_EDITOR
        [ContextMenu(
            "Collect Boarding Points")]
        private void CollectBoardingPoints()
        {
            points =
                GetComponentsInChildren<
                    ShipBoardingPoint>(
                    true);
        }


        private void OnValidate()
        {
            if (points == null)
                return;

            for (int i = 0;
                 i < points.Length;
                 i++)
            {
                ShipBoardingPoint current =
                    points[i];

                if (current == null)
                    continue;

                for (int j = i + 1;
                     j < points.Length;
                     j++)
                {
                    ShipBoardingPoint other =
                        points[j];

                    if (other == null ||
                        current.PointId !=
                        other.PointId)
                    {
                        continue;
                    }

                    Debug.LogError(
                        $"{name}: Boarding Point ID " +
                        $"{current.PointId} is duplicated.",
                        this);
                }
            }
        }
#endif
    }
}
