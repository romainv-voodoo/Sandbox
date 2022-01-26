using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Voodoo.Store
{
    public static class GitignoreHelper
    {
        private static readonly string projectPath = Path.GetDirectoryName(Application.dataPath);
        private static readonly string gitignorePath = projectPath+ "/.gitignore";

        private static readonly string toIgnore = "/Assets/Voodoo/VoodooStore/";
        private static readonly string toIgnoreMeta = "/Assets/Voodoo/VoodooStore.meta";

        private static bool HasGitignore()
        {
            return File.Exists(gitignorePath);
        }
        
        public static bool HasVSTGitignore()
        {
            if (HasGitignore() == false)
            {
                return false;
            }

            string text = GetText();

            return text.IndexOf(toIgnore, StringComparison.Ordinal)>0 && text.IndexOf(toIgnoreMeta, StringComparison.Ordinal)>0;
        }
        
        public static void AddGitignore()
        {
            if (HasVSTGitignore())
            {
                return;
            }
            
            using (StreamWriter streamWriter  = File.AppendText(gitignorePath))
            {
                streamWriter.WriteLine(String.Empty);
                streamWriter.WriteLine(toIgnore);
                streamWriter.WriteLine(toIgnoreMeta);
                streamWriter.Close();
            }
        }

        public static void RemoveGitignore()
        {
            if (HasVSTGitignore()==false)
            {
                return;
            }

            string text = GetText();

            text = text.Replace(toIgnoreMeta, string.Empty)
                .Replace(toIgnore, string.Empty);

            text = Regex.Replace(text, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            
            using (StreamWriter streamWriter = new StreamWriter(gitignorePath))
            {
                streamWriter.Write(text);
                streamWriter.Close();
            }
        }

        private static string GetText()
        {
            string text;
            using (StreamReader streamReader = new StreamReader(gitignorePath))
            {
                text = streamReader.ReadToEnd();
                streamReader.Close();
            }
            return text;
        }
        
    }
}