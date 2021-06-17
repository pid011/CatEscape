using System;

namespace CatEscape.Util
{
    public static class Debug
    {
        public static void Log<T>(T item)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(item);
#endif
        }

        public static void LogError<T>(T item)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(item);
#endif
        }

        public static void LogException(Exception e)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogException(e);
#endif
        }
    }
}
