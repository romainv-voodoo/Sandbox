using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Voodoo.Store
{
    public static class ExporterSerializer
    {
        private const string FileName = "exporterInfo.json";
        
        private static readonly string FilePath = Path.Combine("VoodooStore", FileName);
        
        public static void Read()
        {
            ExporterData data;

            if (File.Exists(FilePath) == false)
            {
                data = new ExporterData();
                DataToExporter(data);
                return;
            }

            string text;
            using (StreamReader reader = File.OpenText(FilePath))
            {
                text = reader.ReadToEnd();
                reader.Close();
            }

            data = JsonUtility.FromJson<ExporterData>(text);

            if (data == null)
            {
                data = new ExporterData();
            }

            DataToExporter(data);
        }

        private static void DataToExporter(ExporterData data)
        {
            Exporter.data = data;
        }

        public static void Write()
        {
            if (Exporter.data == null)
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
                return;
            }
            
            ExporterData data = ExporterToData();

            string text = JsonUtility.ToJson(data, true);
            
            FileInfo fileInfo = new FileInfo(FilePath);
            fileInfo.Directory?.Create();
            File.WriteAllText(FilePath, text);
        }

        private static ExporterData ExporterToData()
        {
            string[] tempAuthors = new string[Exporter.data.authors.Length];
            Array.Copy(Exporter.data.authors, tempAuthors, Exporter.data.authors.Length);
            
            return new ExporterData
            {
                dependencyPackages             = new List<DependencyPackage>(Exporter.data.dependencyPackages),
                labels                         = new List<ExporterLabel>(Exporter.data.labels),
                additionalContents             = new List<ExporterAdditionalContent>(Exporter.data.additionalContents),
                unselectableDependencyPackages = new List<string>(Exporter.data.unselectableDependencyPackages),
                elementsToExport               = new List<string>(Exporter.data.elementsToExport),
                isNewPackage                   = Exporter.data.isNewPackage,
                onlyUpdateInfo                 = Exporter.data.onlyUpdateInfo,
                onlinePackage                  = Exporter.data.onlinePackage,
                package                        = Exporter.data.package,
                newAuthor                      = Exporter.data.newAuthor,
                selectedAuthor                 = Exporter.data.selectedAuthor,
                commitMessage                  = Exporter.data.commitMessage,
                authors                        = tempAuthors
            };
        }
    }
}