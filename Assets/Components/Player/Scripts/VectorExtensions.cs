using UnityEngine;

static public class VectorExtensions
{
    static public Vector3 XZPlane(this Vector3 vec)
    {
        return new Vector3(vec.x, 0, vec.z);
    }
    static public Vector3 YOnly(this Vector3 vec)
    {
        return new Vector3(0, vec.y, 0);
    }
}