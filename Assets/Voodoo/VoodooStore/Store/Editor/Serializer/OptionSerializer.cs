using System.IO;
using UnityEngine;

namespace Voodoo.Store
{
    public static class OptionSerializer
    {
        private static readonly string optionsPath = Path.Combine(PathHelper.DirectoryPath, "Options");

        public static T Read<T>(string path)
        {
            string infoPath = Path.Combine(optionsPath, path + ".json");

            if (File.Exists(infoPath) == false)
            {
                return default;
            }

            string text;
            using (StreamReader reader = File.OpenText(infoPath))
            {
                text = reader.ReadToEnd();
                reader.Close();
            }

            T data = JsonUtility.FromJson<T>(text);

            return data != null ? data : default;
        }

        public static void Write<T>(T data, string path)
        {
            string infoPath = Path.Combine(optionsPath, path + ".json");

            if (data == null)
                return;

            string text = JsonUtility.ToJson(data, true);

            FileInfo fileInfo = new FileInfo(infoPath);
            fileInfo.Directory?.Create();
            File.WriteAllText(infoPath, text);
        }
    }
}
