using System.Collections.Generic;

namespace Voodoo.Store
{ 
    public interface IDownloadable 
    {
        string          Name            { get; }
        int             VersionStatus   { get; }
        List<Package>   Content         { get; }
        List<Package> GetDependencies(List<Package> packageList);
        List<Package> GetRequirements(List<Package> packageList);
    }

    public static class VersionState 
    {
        public const int NotPresent = 0;
        public const int OutDated   = 1;
        public const int UpToDate   = 2;
        public const int Manually   = 4;
        public const int Invalid    = 8;
    }
}