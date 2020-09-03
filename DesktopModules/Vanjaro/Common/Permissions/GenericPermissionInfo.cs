using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using System;
using System.Data;

namespace Vanjaro.Common.Permissions
{
    [Serializable]
    public class GenericPermissionInfo : PermissionInfoBase, IHydratable
    {
        private int _KeyID;

        // Methods
        public GenericPermissionInfo()
        {
            _KeyID = Null.NullInteger;
        }
        public GenericPermissionInfo(PermissionInfo permission)
            : this()
        {
            base.ModuleDefID = permission.ModuleDefID;
            base.PermissionCode = permission.PermissionCode;
            base.PermissionID = permission.PermissionID;
            base.PermissionKey = permission.PermissionKey;
            base.PermissionName = permission.PermissionName;
        }
        public bool Equals(GenericPermissionInfo other)
        {
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || ((((base.AllowAccess == other.AllowAccess)) && (base.RoleID == other.RoleID)) && (base.PermissionID == other.PermissionID)));
        }
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(GenericPermissionInfo))
            {
                return false;
            }
            return Equals((GenericPermissionInfo)obj);
        }
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
        }
        public override int GetHashCode()
        {
            return (_KeyID * 0x18d) ^ 0;
        }

        public int KeyID { get => _KeyID; set => _KeyID = value; }
    }
}