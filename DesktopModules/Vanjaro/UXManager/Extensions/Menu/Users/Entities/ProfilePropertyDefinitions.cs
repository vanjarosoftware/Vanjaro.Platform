using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Profile;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
	public class ProfilePropertyDefinitions
	{
		public bool Visible { get; set; }
		public int ViewOrder { get; set; }
		public string ValidationExpression { get; set; }
		public bool Required { get; set; }
		public bool ReadOnly { get; set; }
		public string PropertyValue { get; set; }
		public string PropertyName { get; set; }
		public int PropertyDefinitionId { get; set; }
		public string PropertyCategory { get; set; }
		public int PortalId { get; set; }
		public int ModuleDefId { get; set; }
		public int Length { get; set; }
		public bool IsDirty { get; set; }
		public bool Deleted { get; set; }
		public UserVisibilityMode DefaultVisibility { get; set; }
		public string DefaultValue { get; set; }
		public int DataType { get; set; }
		public ProfileVisibility ProfileVisibility { get; set; }
		public UserVisibilityMode Visibility { get; set; }
		public ProfilePropertyDefinitions(ProfilePropertyDefinition ProfilePropertyDefinition)
		{
			Visible = ProfilePropertyDefinition.Visible;
			ViewOrder = ProfilePropertyDefinition.ViewOrder;
			ValidationExpression = ProfilePropertyDefinition.ValidationExpression;
			Required = ProfilePropertyDefinition.Required;
			ReadOnly = ProfilePropertyDefinition.ReadOnly;
			PropertyName = ProfilePropertyDefinition.PropertyName;
			PropertyValue = ProfilePropertyDefinition.PropertyValue;
			PropertyDefinitionId = ProfilePropertyDefinition.PropertyDefinitionId;
			PropertyCategory = ProfilePropertyDefinition.PropertyCategory;
			PortalId = ProfilePropertyDefinition.PortalId;
			ModuleDefId = ProfilePropertyDefinition.ModuleDefId;
			Length = ProfilePropertyDefinition.Length;
			IsDirty = ProfilePropertyDefinition.IsDirty;
			DefaultVisibility = ProfilePropertyDefinition.DefaultVisibility;
			DefaultValue = ProfilePropertyDefinition.DefaultValue;
			DataType = ProfilePropertyDefinition.DataType;
			ProfileVisibility = ProfilePropertyDefinition.ProfileVisibility;
			Visibility = ProfilePropertyDefinition.Visibility;
		}
	}
}