using System;
using System.Web;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Services.Localization;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using Vanjaro.UXManager.Extensions.Menu.Users.Controllers;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Factories
{
    public static partial class Factory
    {
        public static class ImportFactory
        {
            public static List<ImportField> GetDataFields(int PortalID, string ImportType, string ImportAction)
            {
                List<ImportField> DataFields = new List<ImportField>();

                switch (ImportType)
                {
                    case "UserAccounts":
                        {
                            ImportField BasicInfoHeader = new ImportField();
                            BasicInfoHeader.Name = "BasicInformation";
                            BasicInfoHeader.DisplayName = "Basic Information";
                            BasicInfoHeader.IsHeader = true;
                            DataFields.Add(BasicInfoHeader);
                            if (!ImportAction.Contains("DeleteUsers") && !ImportAction.Contains("RemoveUsers"))
                            {
                                DataFields.Add(new ImportField("First Name", "Firstname", false, ImportField.DataTypes.String, ""));
                                DataFields.Add(new ImportField("Last Name", "Lastname", false, ImportField.DataTypes.String, ""));
                                DataFields.Add(new ImportField("Display Name", "DisplayName", false, ImportField.DataTypes.String, ""));
                                if (ImportAction == "UpdateUsers")
                                {
                                    DataFields.Add(new ImportField("Email", "Email", false, ImportField.DataTypes.String, ""));
                                    DataFields.Add(new ImportField("User Name", "Username", true, ImportField.DataTypes.String, ""));
                                }
                                else
                                {
                                    DataFields.Add(new ImportField("Email", "Email", true, ImportField.DataTypes.String, ""));
                                    DataFields.Add(new ImportField("User Name", "Username", false, ImportField.DataTypes.String, ""));
                                }
                                DataFields.Add(new ImportField("Password", "Password", false, ImportField.DataTypes.String, ""));

                                ImportField RolesHeader = new ImportField();
                                RolesHeader.Name = "RoleName";
                                RolesHeader.DisplayName = "Security Roles";
                                RolesHeader.IsHeader = true;
                                DataFields.Add(RolesHeader);
                                DataFields.Add(new ImportField("Role Name", "SecurityRoles", false, ImportField.DataTypes.String, "Security Roles"));

                                ImportField PropertiesHeader = new ImportField();
                                PropertiesHeader.Name = "ProfileProperties";
                                PropertiesHeader.DisplayName = "Profile Properties";
                                PropertiesHeader.IsHeader = true;
                                DataFields.Add(PropertiesHeader);


                                List<StringText> ProfileFields = new List<StringText>();
                                List<ProfilePropertyDefinition> pFields;
                                ProfilePropertyDefinitionCollection ppDefinitionCollection = ProfileController.GetPropertyDefinitionsByPortal(PortalID);
                                pFields = ppDefinitionCollection.Cast<ProfilePropertyDefinition>().Where(pp => !pp.Deleted).ToList();
                                foreach (ProfilePropertyDefinition p in pFields)
                                {
                                    DataFields.Add(new ImportField(p.PropertyName, "Profile_" + p.PropertyName, false, ImportField.DataTypes.String, ""));
                                }
                            }
                            else
                            {
                                DataFields.Add(new ImportField("Username", "Username", true, ImportField.DataTypes.String, ""));
                            }
                            break;
                        }
                    case "SecurityRoles":
                        {
                            DataFields.Add(new ImportField("Role Name", "RoleName", true, ImportField.DataTypes.String, ""));

                            if (!ImportAction.Contains("RemoveSecurityRoles"))
                            {
                                DataFields.Add(new ImportField("Description", "Description", false, ImportField.DataTypes.String, ""));
                                DataFields.Add(new ImportField("Group Name", "GroupName", false, ImportField.DataTypes.String, ""));
                                DataFields.Add(new ImportField("Public Role", "PublicRole", false, ImportField.DataTypes.String, ""));
                                DataFields.Add(new ImportField("Auto Assignment", "AutoAssignment", false, ImportField.DataTypes.String, ""));
                            }
                            break;
                        }
                    default:
                        break;
                }
                return DataFields;
            }

            public static List<StringText> ImportType(string ResourceFile)
            {
                List<StringText> ss = new List<StringText>
                {
                    new StringText { Text = "<Select>", Value = "<Select>" },
                    new StringText { Text = Localization.GetString("UserAccounts", ResourceFile), Value = "UserAccounts" },
                    new StringText { Text = Localization.GetString("SecurityRoles", ResourceFile), Value = "SecurityRoles" }
                };
                return ss;

            }

            public static List<StringText> GenerateDisplayName(string ResourceFile)
            {
                List<StringText> ss = new List<StringText>
                {
                    new StringText { Text = Localization.GetString("Emailaddress", ResourceFile), Value = Localization.GetString("Emailaddress", ResourceFile) }
                };
                return ss;

            }

            public static List<StringText> GenerateUserName(string ResourceFile)
            {
                List<StringText> ss = new List<StringText>
                {
                    new StringText { Text = Localization.GetString("Emailaddress", ResourceFile), Value = Localization.GetString("Emailaddress", ResourceFile) }
                };
                return ss;

            }

            public static List<StringText> UserAccount(string ResourceFile)
            {
                List<StringText> ss = new List<StringText>();
                ss.Add(new StringText { Text = Localization.GetString("CreateUsers", ResourceFile), Value = "CreateUsers" });
                ss.Add(new StringText { Text = Localization.GetString("CreateUpdateUsers", ResourceFile), Value = "CreateAndUpdateUsers" });
                ss.Add(new StringText { Text = Localization.GetString("UpdateUsers", ResourceFile), Value = "UpdateUsers" });
                ss.Add(new StringText { Text = Localization.GetString("DeleteUsersSoftDelete", ResourceFile), Value = "DeleteUsers" });
                ss.Add(new StringText { Text = Localization.GetString("RemoveUsers", ResourceFile), Value = "RemoveUsers" });
                return ss;

            }
            public static List<StringText> SecurityRoles(string ResourceFile)
            {
                List<StringText> ss = new List<StringText>();
                ss.Add(new StringText { Text = Localization.GetString("CreateSecurityRoles", ResourceFile), Value = "CreateSecurityRoles" });
                ss.Add(new StringText { Text = Localization.GetString("RemoveSecurityRoles", ResourceFile), Value = "RemoveSecurityRoles" });
                return ss;

            }
        }
    }
}