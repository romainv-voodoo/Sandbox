using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{
    [Serializable]
    public abstract class PackageComposition : IPackageComposition
    {
        [SerializeField] private List<Package> items = new List<Package>();
        [SerializeField] private string name;

        public Package this[int index] { get => items[index]; set => items[index] = value; }
        public int     Count => items.Count;
        public bool    IsReadOnly => false;

        public string  Name        { get => name; set => name = value; }
        public int     minVersion  { get; set; }
        public int     maxVersion  { get; set; }
        public bool    includeDependencies { get; set; }
        public bool    CanBeDeleted => Name != "Favorites" && Name != "Cart";
        
        public List<Package> Content => items;

        public int VersionStatus
        {
            get
            {
                if (items.Count <= 0)
                {
                    return VersionState.Invalid;
                }

                int state = VersionState.UpToDate;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].VersionStatus == VersionState.NotPresent)
                    {
                        return 0;
                    }

                    if (items[i].VersionStatus == VersionState.OutDated)
                    {
                        state = VersionState.OutDated;
                    }
                    else if (items[i].VersionStatus == VersionState.Manually && state != VersionState.OutDated)
                    {
                        state = VersionState.Manually;
                    }
                }

                return state;
            }
        }

        public void Add(Package item) 
        {
            if (item == null || items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == item)
                {
                    return;
                }
            }

            items.Add(item);
        }

        public void AddRange(List<Package> itemsToAdd) 
        {
            if (items == null || itemsToAdd == null || itemsToAdd.Count == 0)
            {
                return;
            }

            foreach (Package package in itemsToAdd)
            {
                Add(package);
            }
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(Package item)
        {
            return items.Find(pkg => pkg.name == item.name) != null;
        }

        public void CopyTo(Package[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public List<Package> GetDependencies(List<Package> packageList)
        {
            List<Package> res = new List<Package>();
            for (int i = 0; i < items.Count; i++)
            {
                res.AddRange(items[i].GetDependencies(packageList));
            }
            
            res.RemoveAll(x => x == null);
            res = res.Distinct().ToList();

            return res;
        }

        public List<Package> GetRequirements(List<Package> packageList)
        {
            List<Package> res = new List<Package>();
            for (int i = 0; i < items.Count; i++)
            {
                res.AddRange(items[i].GetRequirements(packageList));
            }
            
            res.RemoveAll(x => x == null);
            res = res.Distinct().ToList();

            return res;
        }

        public IEnumerator<Package> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(Package item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item.name)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, Package item)
        {
            items.Insert(index, item);
        }

        public void Remove(Package item)
        {
            int index = IndexOf(item);
            
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        bool ICollection<Package>.Remove(Package item)
        {
            int index = IndexOf(item);

            if (index >= 0)
            {
                RemoveAt(index);
            }

            return index >= 0;
        }
    }
}