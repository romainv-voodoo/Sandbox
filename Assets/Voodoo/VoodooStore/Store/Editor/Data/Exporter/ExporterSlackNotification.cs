using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voodoo.Store
{
    public static class ExporterSlackNotification
    {
        private const string hooksUrl = "https://hooks.slack.com/services/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";

        private static ExporterData data;
        private static Package newPackage;
        private static Package oldPackage;
        
        public static async Task SendUpdateNotification()
        {
            data = Exporter.data;
            
            if (data == null)
            {
                return;
            }
            
            if (User.isGTD)
            {
                return;
            }
            
            newPackage = data.package;
            oldPackage = data.onlinePackage;

            await SlackWebHook.PostToSlack(hooksUrl, BuildParts());
        }

        private static string BuildParts()
        {
            List<string> parts = new List<string>();
            
            parts.Add(SlackBlockHelper.Header("New Update on the Voodoo Store  :tada:"));

            parts.Add(SlackBlockHelper.Divider());

            parts.Add(SlackBlockHelper.Body(data.isNewPackage ? NewPackage() : PackageUpdate()));
            
            parts.Add(SlackBlockHelper.Divider());

            string docURL = newPackage.documentationLink;

            if (string.IsNullOrEmpty(docURL))
            {
                parts.Add(SlackBlockHelper.Body("There is no documentation for this package :disappointed_relieved:"));
            }
            else
            {
                bool result = Uri.TryCreate(docURL, UriKind.Absolute, out Uri uriResult) 
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                parts.Add(result
                    ? SlackBlockHelper.ButtonURL(docURL, "Go review the documentation")
                    : SlackBlockHelper.Body(
                        $"The documentation is not a valid url :disappointed_relieved: \n ({docURL.ToItalic()})"));
            }
            
            return SlackBlockHelper.BuildContent(parts);
        }

        private static string NewPackage()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"New package {newPackage.Name.ToBold()} by {newPackage.author.ToBold()}");
            sb.AppendLine();
            sb.Append("Version: "+newPackage.version);
            sb.AppendLine();
            sb.Append(newPackage.description.ToQuote());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();

            if (newPackage?.dependencies.Count > 0)
            {
                sb.Append("Dependencies:".ToBold());
                sb.AppendLine();
                sb.Append(newPackage.dependencies.ToBulletList());
                sb.AppendLine();
                sb.AppendLine();
            }
            
            if (newPackage?.labels.Count > 0)
            {
                sb.Append("Labels:".ToBold());
                sb.AppendLine();
                sb.Append(newPackage.labels.ToBulletList());
                sb.AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }
        
        private static string PackageUpdate()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append("Update of " + newPackage.Name.ToBold());
            sb.AppendLine();
            sb.Append($" by {GitHubUtility.ClientName.ToItalic()}  Author: {newPackage.author.ToItalic()}");
            sb.AppendLine();
            sb.AppendLine();

            if (data.onlyUpdateInfo)
            {
                sb.AppendLine();
                sb.Append(" :round_pushpin: This update only change the following information and not the package content");
                sb.AppendLine();
            }

            if (!String.Equals(oldPackage.version, newPackage.version))
            {
                sb.AppendLine();
                sb.Append(oldPackage.version.ToStrike() + " => "+  newPackage.version);
                sb.AppendLine();
            }

            if (String.Equals(newPackage.description, oldPackage.description))
            {
                sb.AppendLine();
                sb.Append(newPackage.description.ToQuote());
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine();
                sb.Append("Old description".ToBold());
                sb.AppendLine();
                sb.Append(oldPackage.description.ToQuote());
                sb.AppendLine();
                sb.Append("New description".ToBold());
                sb.AppendLine();
                sb.Append(newPackage.description.ToQuote());
                sb.AppendLine();
            }
            sb.AppendLine();

            List<string> newDependencies = newPackage.dependencies.Except(oldPackage.dependencies).ToList();
            List<string> removedDependencies = oldPackage.dependencies.Except(newPackage.dependencies).ToList();

            if (newDependencies.Any() || removedDependencies.Any())
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.Append("Changes in Dependencies".ToBold());
                sb.AppendLine();
                sb.AppendLine();
                
                if (newDependencies.Count > 0)
                {
                    sb.Append("added:".ToItalic());
                    sb.AppendLine();
                    sb.Append(newDependencies.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }
               
                
                List<string> dependenciesUnchanged = new List<string>(newPackage.dependencies);
                dependenciesUnchanged = dependenciesUnchanged.Except(newDependencies).ToList();
                dependenciesUnchanged.Sort();

                if (dependenciesUnchanged.Count > 0)
                {
                    sb.Append("unchanged:".ToItalic());
                    sb.AppendLine();
                    sb.Append(dependenciesUnchanged.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (removedDependencies.Count > 0)
                {
                    sb.Append("removed:".ToItalic());
                    sb.AppendLine();
                    sb.Append(dependenciesUnchanged.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }
                
            }
            
            List<string> newLabels = newPackage.labels.Except(oldPackage.labels).ToList();
            List<string> removedLabels = oldPackage.labels.Except(newPackage.labels).ToList();

            if (newLabels.Any() || removedDependencies.Any())
            {
                sb.AppendLine();
                sb.Append("Changes in Labels".ToBold());
                sb.AppendLine();
                sb.AppendLine();
                
                if (newLabels.Count > 0)
                {
                    sb.Append("added:".ToItalic());
                    sb.AppendLine();
                    sb.Append(newLabels.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }

                List<string> labelsUnchanged = new List<string>(newPackage.labels);
                labelsUnchanged = labelsUnchanged.Except(newLabels).ToList();
                labelsUnchanged.Sort();
                
                if (labelsUnchanged.Count > 0)
                {
                    sb.Append("unchanged:".ToItalic());
                    sb.AppendLine();
                    sb.Append(labelsUnchanged.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (removedLabels.Count > 0)
                {
                    sb.Append("removed:".ToItalic());
                    sb.AppendLine();
                    sb.Append(removedLabels.ToBulletList());
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
            sb.AppendLine();

            sb.Append("Commit Message".ToBold());
            if (String.IsNullOrEmpty(data.commitMessage))
            {
                sb.AppendLine();
                sb.Append("No commit message!  :rage:");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine();
                sb.Append(data.commitMessage.ToQuote());
                sb.AppendLine();
            }
            
            return sb.ToString();
        }
    }
}