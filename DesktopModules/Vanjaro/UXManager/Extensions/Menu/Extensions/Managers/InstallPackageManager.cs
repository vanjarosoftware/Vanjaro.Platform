using Dnn.PersonaBar.Extensions.Components;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Installer.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Managers
{
    public class InstallPackageManager
    {
        public static List<ParseResultDto> ParsePackage(PortalSettings PortalSettings, UserInfo UserInfo)
        {
            List<ParseResultDto> ParseResults = new List<ParseResultDto>();
            foreach (KeyValuePair<string, PackageInfo> item in GetInstallPackages())
            {
                using (FileStream stream = new FileStream(item.Key, FileMode.Open))
                {
                    try
                    {
                        ParseResults.Add(InstallController.Instance.ParsePackage(PortalSettings, UserInfo, item.Key, stream));
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }

            }
            return ParseResults;
        }

        internal static void InstallPackage(PortalSettings portalSettings, UserInfo userInfo)
        {
            foreach (KeyValuePair<string, PackageInfo> item in GetInstallPackages())
            {
                using (FileStream stream = new FileStream(item.Key, FileMode.Open))
                {
                    try
                    {
                        InstallController.Instance.InstallPackage(portalSettings, userInfo, null, item.Key, stream);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }

            }
        }

        internal static void DeletePackage(PortalSettings portalSettings, UserInfo userInfo)
        {
            string installPackagePath = Globals.ApplicationMapPath + "\\Install\\Module";
            if (!Directory.Exists(installPackagePath))
            {
                return;
            }

            string[] files = Directory.GetFiles(installPackagePath);
            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    FileWrapper.Instance.Delete(file);
                }
            }
        }

        public static IDictionary<string, PackageInfo> GetInstallPackages()
        {
            List<string> invalidPackages = new List<string>();

            Dictionary<string, PackageInfo> packages = new Dictionary<string, PackageInfo>();

            ParsePackagesFromApplicationPath(packages, invalidPackages);

            //Add packages with no dependency requirements
            Dictionary<string, PackageInfo> sortedPackages = packages.Where(p => p.Value.Dependencies.Count == 0).ToDictionary(p => p.Key, p => p.Value);

            int prevDependentCount = -1;

            Dictionary<string, PackageInfo> dependentPackages = packages.Where(p => p.Value.Dependencies.Count > 0).ToDictionary(p => p.Key, p => p.Value);
            int dependentCount = dependentPackages.Count;
            while (dependentCount != prevDependentCount)
            {
                prevDependentCount = dependentCount;
                List<string> addedPackages = new List<string>();
                foreach (KeyValuePair<string, PackageInfo> package in dependentPackages)
                {
                    if (package.Value.Dependencies.All(
                            d => sortedPackages.Any(p => p.Value.Name.Equals(d.PackageName, StringComparison.OrdinalIgnoreCase) && p.Value.Version >= d.Version)))
                    {
                        sortedPackages.Add(package.Key, package.Value);
                        addedPackages.Add(package.Key);
                    }
                }
                foreach (string packageKey in addedPackages)
                {
                    dependentPackages.Remove(packageKey);
                }
                dependentCount = dependentPackages.Count;
            }

            //Add any packages whose dependency cannot be resolved
            foreach (KeyValuePair<string, PackageInfo> package in dependentPackages)
            {
                sortedPackages.Add(package.Key, package.Value);
            }

            return sortedPackages;
        }

        private static void ParsePackagesFromApplicationPath(Dictionary<string, PackageInfo> packages, List<string> invalidPackages)
        {

            string installPackagePath = Globals.ApplicationMapPath + "\\Install\\Module";
            if (!Directory.Exists(installPackagePath))
            {
                return;
            }

            string[] files = Directory.GetFiles(installPackagePath);
            if (files.Length <= 0)
            {
                return;
            }

            Array.Sort(files); // The order of the returned file names is not guaranteed on certain NAS systems; use the Sort method if a specific sort order is required.

            List<string> optionalPackages = new List<string>();
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file.ToLowerInvariant());
                if (extension != ".zip")
                {
                    continue;
                }

                PackageController.ParsePackage(file, installPackagePath, packages, invalidPackages);
                if (packages.ContainsKey(file))
                {
                    //check whether have version conflict and remove old version.
                    PackageInfo package = packages[file];

                    PackageInfo installedPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger,
                        p => p.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase)
                                && p.PackageType.Equals(package.PackageType, StringComparison.OrdinalIgnoreCase));

                    if (packages.Values.Count(p => p.FriendlyName.Equals(package.FriendlyName, StringComparison.OrdinalIgnoreCase)) > 1
                            || installedPackage != null)
                    {
                        List<KeyValuePair<string, PackageInfo>> oldPackages = packages.Where(kvp => kvp.Value.FriendlyName.Equals(package.FriendlyName, StringComparison.OrdinalIgnoreCase)
                                                                    && kvp.Value.Version < package.Version).ToList();

                        //if there already have higher version installed, remove current one from list.
                        if (installedPackage != null && package.Version < installedPackage.Version)
                        {
                            oldPackages.Add(new KeyValuePair<string, PackageInfo>(file, package));
                        }

                        if (oldPackages.Any())
                        {
                            foreach (KeyValuePair<string, PackageInfo> oldPackage in oldPackages)
                            {
                                try
                                {
                                    packages.Remove(oldPackage.Key);
                                    FileWrapper.Instance.Delete(oldPackage.Key);
                                }
                                catch (Exception)
                                {
                                    //do nothing here.
                                }
                            }
                        }
                    }
                }

                if (extension != ".zip")
                {
                    optionalPackages.Add(file);
                }
            }

            //remove optional
            optionalPackages.ForEach(f =>
            {
                if (packages.ContainsKey(f))
                {
                    packages.Remove(f);
                }
            });
        }

    }
}