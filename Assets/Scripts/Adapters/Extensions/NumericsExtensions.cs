using UnityEngine;

public static class NumericsExtensions
{
    public static Vector2 ToUnityVector(this System.Numerics.Vector2 v)
        => new Vector2(v.X, v.Y);

    public static Vector3 ToUnityVector(this System.Numerics.Vector3 v)
        => new Vector3(v.X, v.Y, v.Z);

    public static Vector4 ToUnityVector(this System.Numerics.Vector4 v)
        => new Vector4(v.X, v.Y, v.Z, v.W);

    public static Quaternion ToUnityQuaternion(this System.Numerics.Quaternion q)
        => new Quaternion(q.X, q.Y, q.Z, q.W);

    public static System.Numerics.Vector2 ToNumericsVector(this Vector2 v)
        => new System.Numerics.Vector2(v.x, v.y);

    public static System.Numerics.Vector3 ToNumericsVector(this Vector3 v)
        => new System.Numerics.Vector3(v.x, v.y, v.z);

    public static System.Numerics.Vector4 ToNumericsVector(this Vector4 v)
        => new System.Numerics.Vector4(v.x, v.y, v.z, v.w);

    public static System.Numerics.Quaternion ToNumericsQuaternion(this Quaternion q)
        => new System.Numerics.Quaternion(q.x, q.y, q.z, q.w);
}

