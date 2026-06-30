using UnityEngine;

namespace Assets.scripts.Infrastructure
{
    public static class PlatformService
    {
        public static void Quit()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("window.close(); window.location.href = \"about:blank\";");
#else
            Application.Quit();
#endif
        }

        public static string GetPathForSaving()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Application.persistentDataPath;
#else
            return Application.dataPath;
#endif
        }
    }
}