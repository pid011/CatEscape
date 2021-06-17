using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace CatEscape
{
    public static class Startup
    {
        private static bool s_serializerRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (!s_serializerRegistered)
            {
                StaticCompositeResolver.Instance.Register(GeneratedResolver.Instance, StandardResolver.Instance);

                var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
                MessagePackSerializer.DefaultOptions = option;
                s_serializerRegistered = true;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            Initialize();
        }
#endif
    }
}
