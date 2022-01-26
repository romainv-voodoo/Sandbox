using System.Collections.Generic;

namespace Voodoo.Store
{
    [System.Serializable]
    public class VoodooStoreData
    {
        public PackagePreset        cart        = PackagePreset.cart;
        public PackagePreset        favorites   = PackagePreset.favorites;
        public List<PackagePreset>  presets     = new List<PackagePreset>();
        public List<Package>        packageList = new List<Package>();
    }
}