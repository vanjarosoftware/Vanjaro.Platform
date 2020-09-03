using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Vanjaro.Common.Utilities
{
    public class UserUtils
    {
        private const string GravatarURL = "http://www.gravatar.com";
        private const string GravatarSecureURL = "https://secure.gravatar.com";

        public static string GetHttpAlias(int PortalID)
        {
            if (HttpContext.Current != null)
            {
                string test = HttpContext.Current.Request.IsSecureConnection ? "https://" : "http://";
                return test + HttpContext.Current.Request.Url.Host + HttpContext.Current.Request.ApplicationPath;
            }
            else
            {
                IEnumerable<PortalAliasInfo> portalinfos = (new PortalAliasController()).GetPortalAliasesByPortalId(PortalID);
                if (portalinfos != null && portalinfos.Count() > 0)
                {
                    return portalinfos.ElementAt(0).HTTPAlias;
                }
            }
            return "";
        }

        public static string GetImageUrl(int PortalID, string Email)
        {
            try
            {
                int ImageWidth = 80;
                bool SSL = false;
                if (!string.IsNullOrEmpty(GetHttpAlias(PortalID)) && GetHttpAlias(PortalID).ToLower().StartsWith("https"))
                {
                    SSL = true;
                }

                
                string Hash = Utils.CreateMD5(Email.Trim().ToLower()).ToLower();

                string ImageURL = string.Empty;

                if (SSL)
                {
                    ImageURL += GravatarSecureURL;
                }
                else
                {
                    ImageURL += GravatarURL;
                }

                ImageURL += "/avatar/" + Hash + ".jpg?d=mp&s=" + ImageWidth.ToString();

                return ImageURL;
            }
            catch (Exception) { }
            return null;
        }

        public static string GetProfileImage(int PortalID, string Email)
        {
            return GetProfileImage(PortalID, -1, Email);
        }

        public static string GetProfileImage(int PortalID, int UserID)
        {
            UserInfo uInfo = UserController.GetUserById(PortalID, UserID);
            string Email = null;
            if (uInfo != null)
            {
                Email = uInfo.Email;
            }

            return GetProfileImage(PortalID, UserID, Email);
        }

        public static string GetProfileImage(int PortalID, int UserID, string Email)
        {
            return GetProfileImage(PortalID, UserID, Email, 120, 120);
        }

        public static string GetProfileImage(int PortalID, int UserID, string Email, int Width, int Height)
        {
            string PhotoURL = string.Empty;
            Email = Email == null ? "" : Email;
            if (UserID > -1)
            {
                UserInfo uInfo = UserController.GetUserById(PortalID, UserID);
                if (uInfo != null)
                {
                    var pp = uInfo.Profile.GetProperty("ProfilePictureURL");

                    if (pp != null && !string.IsNullOrEmpty(pp.PropertyValue))
                        PhotoURL = pp.PropertyValue;
                    else if (DotNetNuke.Common.Globals.DataBaseVersion.Major <= 7)
                    {
                        PhotoURL = uInfo.Profile.PhotoURL;
                    }
                    else
                    {
                        if (!uInfo.Profile.PhotoURL.Contains("no_avatar.gif"))
                        {
                            PhotoURL = VirtualPathUtility.ToAbsolute((string.Format("~/DnnImageHandler.ashx?mode=profilepic&userId={0}&h={1}&w={2}", uInfo.UserID, Height, Width)));
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(PhotoURL) && !PhotoURL.Contains("no_avatar.gif"))
            {
                return PhotoURL;
            }

            string Url = GetImageUrl(PortalID, Email);
            if (!string.IsNullOrEmpty(Url))
            {
                PhotoURL = Url;
            }

            if (UserID == -1 && string.IsNullOrEmpty(PhotoURL))
            {
                PhotoURL = VirtualPathUtility.ToAbsolute("~/DesktopModules/Vanjaro/Common/Resources/Images/no_avatar.gif");
            }

            return PhotoURL;
        }
    }
}