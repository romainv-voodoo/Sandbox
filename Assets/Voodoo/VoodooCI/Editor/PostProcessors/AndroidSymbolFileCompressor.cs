using UnityEditor;
using UnityEditor.Callbacks;

namespace Voodoo.CI
{
    public static class AndroidSymbolFileCompressor
    {
        [PostProcessBuild(1)]
        public static void ProcessSymbolFiles(BuildTarget target, string pathToBuildProject)
        {
            if (target == BuildTarget.Android)
            {
                BuildPipelineBase.CompressSymbolFiles();
            }
        }
    }
}