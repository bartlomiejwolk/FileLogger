using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// todo add namespace
public static class Utilities {

    /// <summary>
    /// </summary>
    /// <remarks>http://forum.unity3d.com/threads/assert-class-for-debugging.59010/</remarks>
    /// <param name="assertion"></param>
    /// <param name="assertString"></param>
    [Conditional("UNITY_EDITOR")]
    public static void Assert(Func<bool> assertion, string assertString) {
        if (!assertion()) {
            var myTrace = new StackTrace(true);
            var myFrame = myTrace.GetFrame(1);
            var assertInformation = "Filename: " + myFrame.GetFileName()
                                    + "\nMethod: " + myFrame.GetMethod()
                                    + "\nLine: "
                                    + myFrame.GetFileLineNumber();

            // Output message to Unity log window.
            UnityEngine.Debug.Log(assertString + "\n" + assertInformation);
            // Break only in play mode.
            if (Application.isPlaying) {
                UnityEngine.Debug.Break();
            }
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog(
                "Assert!",
                assertString + "\n" + assertInformation,
                "Close")) {
            }
#endif
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    /// <remarks>http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp</remarks>
    public static bool FloatsEqual(float a, float b, float epsilon) {
        var absA = Math.Abs(a);
        var absB = Math.Abs(b);
        var diff = Math.Abs(a - b);

        if (a == b) {
            // shortcut, handles infinities
            return true;
        }
        if (a == 0 || b == 0) {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < (epsilon * Single.MinValue);
        }
        // use relative error
        return diff / (absA + absB) < epsilon;
    }

    public static object InvokeMethodWithReflection(
        object target,
        string methodName,
        object[] parameters) {

        // Get method metadata.
        var methodInfo = target.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        var result = methodInfo.Invoke(target, parameters);

        return result;
    }

}
