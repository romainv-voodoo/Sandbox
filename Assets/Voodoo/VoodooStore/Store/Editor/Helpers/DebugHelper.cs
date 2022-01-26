using System.IO;

namespace Voodoo.Store
{
    public static class DebugHelper 
    {
        public static void DumpWebResponse(string path, string content) 
        {
            string infoPath = Path.Combine(PathHelper.DirectoryPath, path);

            string directory = infoPath.Substring(0, infoPath.LastIndexOf(Path.DirectorySeparatorChar));
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(infoPath, content);
        }

    }
}