using UnityEngine;

public static class GameUtility
{
    public static Vector3 ToVector3XZ(this Vector2 v)
    {
        return new Vector3(v.x, 0f, v.y);
    }

    public static Vector3 ToVector3XZ(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }
}