using System;
using System.IO;
using UnityEngine;

namespace Assets.scripts.Core
{
    /// <summary>
    /// abstract class for saving data in webgl or other platforms.
    /// </summary>
    /// <typeparam name="T">Тип данных для сохранения (должен быть сериализуемым через JsonUtility)</typeparam>
    public abstract class FileStorage<T> where T : class, new()
    {
        protected abstract string FileName { get; }
        protected abstract T GetDefaultData();

        public virtual T Load()
        {
            string path = GetFilePath();

#if UNITY_WEBGL && !UNITY_EDITOR
            string json = PlayerPrefs.GetString(FileName, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    T data = JsonUtility.FromJson<T>(json);
                    if (data != null)
                        return data;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to parse {FileName}: {e.Message}");
                }
            }
            return GetDefaultData();
#else
            if (!File.Exists(path))
            {
                return GetDefaultData();
            }

            try
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);
                return data ?? GetDefaultData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load {FileName}: {e.Message}");
                return GetDefaultData();
            }
#endif
        }

        public virtual void Save(T data)
        {
            if (data == null)
            {
                Debug.LogError($"Cannot save null data to {FileName}");
                return;
            }

            string json = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
            // В WebGL сохраняем в PlayerPrefs
            PlayerPrefs.SetString(FileName, json);
            PlayerPrefs.Save();
#else
            try
            {
                string path = GetFilePath();
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {FileName}: {e.Message}");
            }
#endif
        }

        public virtual bool HasSavedData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return PlayerPrefs.HasKey(FileName);
#else
            return File.Exists(GetFilePath());
#endif
        }

        public virtual void DeleteSavedData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.DeleteKey(FileName);
            PlayerPrefs.Save();
#else
            string path = GetFilePath();
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to delete {FileName}: {e.Message}");
                }
            }
#endif
        }

        protected virtual string GetFilePath()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Path.Combine(Application.persistentDataPath, FileName);
#else
            return Path.Combine(Application.dataPath, FileName);
#endif
        }
    }
}