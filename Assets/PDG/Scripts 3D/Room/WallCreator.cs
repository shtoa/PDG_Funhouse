using UnityEngine;

namespace dungeonGenerator
{
    public class WallCreator
    {
        private void DrawWall(BoundsInt wallBound, Transform transform, Material wallMaterial)
        {
            GameObject wall = MeshHelper.CreateCuboid(wallBound.size, 1);
            wall.transform.SetParent(transform, false);

            wall.transform.localPosition = wallBound.center;
            wall.GetComponent<MeshRenderer>().material = wallMaterial;

        }
    }
}