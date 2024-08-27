using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace Playmove
{
    public class Util
    {
#if UNITY_EDITOR
        public static void ClearDebugLog()
        {
            Assembly SampleAssembly = Assembly.GetAssembly(typeof(UnityEditorInternal.ComponentUtility));
            if (SampleAssembly == null)
            {
                Debug.LogError("!!! (SampleAssembly == null)");
            }
            else
            {
                Type type = SampleAssembly.GetType("UnityEditorInternal.LogEntries");
                if (type == null)
                {
                    Debug.LogError("!!! (type == null)");
                }
                else
                {
                    MethodInfo method = type.GetMethod("Clear");
                    if (method == null)
                    {
                        Debug.LogError("!!! (method == null)");
                    }
                    else
                    {
                        method.Invoke(new object(), null);
                    }
                }
            }
        }
#endif
    }
}