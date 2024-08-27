using UnityEngine;
using System.Collections;

public static class PYExtensionMethods
{
    #region Bounds
    public static bool Contains2D(this Bounds b, Vector3 point)
    {
        point.z = b.center.z;
        return b.Contains(point);
    }

    public static bool Intersects2D(this Bounds b, Bounds bounds)
    {

        return !(bounds.min.x > b.max.x
            || bounds.max.x < b.min.x
            || bounds.max.y < b.min.y
            || bounds.min.y > b.max.y);
    }
    #endregion

    #region Object
#if !UNITY_5_4_OR_NEWER
    public static Object InstantiateParent(this Object o, Object original, Transform parent)
    {
        return o.InstantiateParent(original, parent, false);
    }

    public static Object InstantiateParent(this Object o, Object original, Transform parent, bool worldPositionStays)
    {
        Object obj = Object.Instantiate(original);
        MonoBehaviour mono = (MonoBehaviour)obj;
        mono.transform.SetParent(parent, worldPositionStays);

        return mono;
    }
#endif

    public static Object InstantiateName(this Object o, Object original, string name)
    {
        Object obj = Object.Instantiate(original);
        obj.name = name;

        return obj;
    }

    //public static Object InstantiateName(this Object o, Object original, string name, Transform parent)
    //{
    //    Object obj = o.InstantiateParent(original, parent);
    //    obj.name = name;

    //    return obj;
    //}

    //public static Object InstantiateName(this Object o, Object original, string name, Transform parent, bool worldPositionStays)
    //{
    //    Object obj = o.InstantiateParent(original, parent, worldPositionStays);
    //    obj.name = name;

    //    return obj;
    //}

    public static Object InstantiateName(this Object o, Object original, string name, Vector3 position, Quaternion rotation)
    {
        Object obj = Object.Instantiate(original, position, rotation);
        obj.name = name;

        return obj;
    }
    #endregion

    #region Transform
    public static void LookAt2D(this Transform trans, Vector3 targetPosition)
    {
        var dir = targetPosition - trans.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public static void LookAt2D(this Transform trans, Transform targetTransform)
    {
        var dir = targetTransform.position - trans.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public static void LookAt2DSmooth(this Transform trans, Vector3 targetPosition, float rotationSpeed)
    {
        var dir = targetPosition - trans.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var rot = Quaternion.Slerp(trans.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
        trans.eulerAngles = new Vector3(trans.eulerAngles.x, trans.eulerAngles.y, rot.eulerAngles.z);
    }
    #endregion
}
