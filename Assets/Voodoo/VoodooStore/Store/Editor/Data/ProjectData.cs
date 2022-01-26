using System.Collections.Generic;

namespace Voodoo.Store
{
    [System.Serializable]
    public class ProjectData
    {
        public List<string> packagesNames   = new List<string>();
        public List<string> packagesVersion = new List<string>();
    }
}