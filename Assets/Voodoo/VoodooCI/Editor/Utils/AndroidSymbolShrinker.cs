using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Voodoo.CI
{
    public class AndroidSymbolShrinker
    {
        private static string LastSymbolToShrinkLocation = nameof(LastSymbolToShrinkLocation);

        private class ProcessResult
        {
            private int ExitCode { get; }
            private string StdOut { get; }
            private string StdErr { get; }

            internal bool Failure => ExitCode != 0;

            internal ProcessResult(int exitCode, string stdOut, string stdErr)
            {
                ExitCode = exitCode;
                StdOut = stdOut;
                StdErr = stdErr;
            }

            public override string ToString()
            {
                return $"Exit Code: {ExitCode}\nStdOut:\n{StdOut}\nStdErr:\n{StdErr}";
            }
        }

        private static void Log(string message)
        {
            UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $":: :: :: {message} :: :: ::");
        }

        private static ProcessResult RunProcess(string workingDirectory, string fileName, string args)
        {
            Log($"Executing {fileName} {args} (Working Directory: {workingDirectory}");
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.CreateNoWindow = true;
            var output = new StringBuilder();
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                }
            });

            var error = new StringBuilder();
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    error.AppendLine(e.Data);
                }
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Log($"{fileName} exited with {process.ExitCode}");
            return new ProcessResult(process.ExitCode, output.ToString(), error.ToString());
        }

        private static void Cleanup(string path)
        {
            if (Directory.Exists(path))
            {
                Log($"Delete {path}");
                Directory.Delete(path, true);
            }

            if (File.Exists(path))
            {
                Log($"Delete {path}");
                File.Delete(path);
            }
        }

        public static void ShrinkSymbols(string location, string newZipName = null)
        {
            var zipApplicationLocation = FindZipFileApp();

            var targetDirectory = Path.GetDirectoryName(location);
            var intermediatePath = Path.Combine(targetDirectory, "TempShrink");

            if (newZipName == null)
            {
                newZipName = Path.Combine(targetDirectory, Path.GetFileNameWithoutExtension(location) + ".small.zip");
            }
            else
            {
                newZipName = Path.Combine(targetDirectory, newZipName);
            }

            EditorPrefs.SetString(LastSymbolToShrinkLocation, targetDirectory);

            Cleanup(intermediatePath);
            Cleanup(newZipName);
            var result = RunProcess(targetDirectory, zipApplicationLocation,
                $"x -o\"{intermediatePath}\" \"{location}\"");
            if (result.Failure)
                throw new Exception(result.ToString());

            EditorUtility.DisplayProgressBar("Shrinking symbols", "Deleting/Renaming/Compressing symbol files", 0.5f);
            var files = Directory.GetFiles(intermediatePath, "*.*", SearchOption.AllDirectories);
            var symSo = ".sym.so";
            foreach (var file in files)
            {
                if (file.EndsWith(".dbg.so"))
                    Cleanup(file);
                if (file.EndsWith(symSo))
                {
                    var fileSO = file.Substring(0, file.Length - symSo.Length) + ".so";
                    Log($"Rename {file} --> {fileSO}");
                    File.Move(file, fileSO);
                }
            }

            result = RunProcess(intermediatePath, zipApplicationLocation, $"a -tzip \"{newZipName}\"");
            EditorUtility.ClearProgressBar();
            if (result.Failure)
                throw new Exception(result.ToString());

            Cleanup(intermediatePath);

            Log($"New small symbol package: {newZipName}");
            EditorUtility.RevealInFinder(newZipName);
        }

        private static string FindZipFileApp()
        {
            var zipFileName = Path.GetFullPath(Path.Combine(EditorApplication.applicationContentsPath, "Tools", "7z"));
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                zipFileName += ".exe";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.LinuxEditor)
            {
                zipFileName = Path.GetFullPath(Path.Combine(EditorApplication.applicationContentsPath, "Tools", "7za"));
            }

            if (!File.Exists(zipFileName))
            {
                throw new Exception($"Failed to locate {zipFileName}.");
            }

            return zipFileName;
        }
    }
}