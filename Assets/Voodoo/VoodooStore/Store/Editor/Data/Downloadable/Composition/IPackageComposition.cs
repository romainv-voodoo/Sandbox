using System.Collections.Generic;

namespace Voodoo.Store
{ 
    public interface IPackageComposition : IList<Package>, IDownloadable
    {
        int minVersion { get; set; }
        int maxVersion { get; set; }

        bool includeDependencies { get; set; }
    }
}