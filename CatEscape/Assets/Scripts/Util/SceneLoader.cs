namespace CatEscape.Util
{
    public sealed class SceneLoader : Singleton<SceneLoaderComponent>
    {
        public static void LoadScene(string sceneName)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.LoadScene(sceneName);
        }
    }
}
