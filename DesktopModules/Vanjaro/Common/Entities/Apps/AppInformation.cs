using System.Collections.Generic;

namespace Vanjaro.Common.Entities.Apps
{
    public class AppInformation
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string ID { get; set; }
        public string GetRuntimeVersion { get; set; }
        public string PurchaseURL { get; set; }
        public string ActivationHelpURL { get; set; }
        public int TrialDays { get; set; }
        public int ValidateDays { get; set; }
        public List<string> SupportedEditions { get; set; }
        public bool ImplementLicensing { get; set; }
        public string LicenseFilePath { get; set; }

        public AppInformation(string name, string id, string runtimeVersion, string purchaseURL, string activationHelpURL, int trialDays, int validateDays, List<string> supportedEditions, bool implementLicensing)
        {
            Name = name;
            FriendlyName = name;
            ID = id;
            GetRuntimeVersion = runtimeVersion;
            TrialDays = trialDays;
            ValidateDays = validateDays;
            PurchaseURL = purchaseURL;
            ActivationHelpURL = activationHelpURL;
            SupportedEditions = supportedEditions;
            ImplementLicensing = implementLicensing;
            LicenseFilePath = "~/Desktopmodules/" + name;
        }
        public AppInformation(string name, string friendlyName, string id, string runtimeVersion, string purchaseURL, string activationHelpURL, int trialDays, int validateDays, List<string> supportedEditions, bool implementLicensing)
        {
            Name = name;
            FriendlyName = friendlyName;
            ID = id;
            GetRuntimeVersion = runtimeVersion;
            TrialDays = trialDays;
            ValidateDays = validateDays;
            PurchaseURL = purchaseURL;
            ActivationHelpURL = activationHelpURL;
            SupportedEditions = supportedEditions;
            ImplementLicensing = implementLicensing;
            LicenseFilePath = "~/Desktopmodules/" + name;
        }
        public AppInformation(string name, string friendlyName, string id, string runtimeVersion, string purchaseURL, string activationHelpURL, int trialDays, int validateDays, List<string> supportedEditions, bool implementLicensing, string licenseFilePath)
        {
            Name = name;
            FriendlyName = friendlyName;
            ID = id;
            GetRuntimeVersion = runtimeVersion;
            TrialDays = trialDays;
            ValidateDays = validateDays;
            PurchaseURL = purchaseURL;
            ActivationHelpURL = activationHelpURL;
            SupportedEditions = supportedEditions;
            ImplementLicensing = implementLicensing;
            LicenseFilePath = licenseFilePath + name;
        }
        public AppInformation(string name, string friendlyName, string id, string runtimeVersion, string purchaseURL, string activationHelpURL, int trialDays, int validateDays, List<string> supportedEditions, bool implementLicensing, string licenseFilePath, string licenseFolderName)
        {
            Name = name;
            FriendlyName = friendlyName;
            ID = id;
            GetRuntimeVersion = runtimeVersion;
            TrialDays = trialDays;
            ValidateDays = validateDays;
            PurchaseURL = purchaseURL;
            ActivationHelpURL = activationHelpURL;
            SupportedEditions = supportedEditions;
            ImplementLicensing = implementLicensing;
            LicenseFilePath = licenseFilePath + licenseFolderName;
        }
    }
}