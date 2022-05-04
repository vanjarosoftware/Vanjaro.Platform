using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using System;
using System.Linq;
using System.Net.Mail;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Factories;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public class LoginFactory
        {
            internal static bool SendVerificationCode(PortalSettings PortalSettings, string Email)
            {
                try
                {
                    EmailVerification emailVerification = EmailVerification.Query("where Email=@0 and PortalID=@1", Email, PortalSettings.PortalId).FirstOrDefault();
                    if (emailVerification != null)
                    {
                        emailVerification.OTP = new Random().Next(100000, 999999);
                        emailVerification.CreatedOn = DateTime.UtcNow;
                        emailVerification.Update();
                        SendMail(PortalSettings, Email, emailVerification.OTP);
                    }
                    else
                    {
                        emailVerification = new EmailVerification();
                        emailVerification.Email = Email;
                        emailVerification.PortalID = PortalSettings.PortalId;
                        emailVerification.OTP = new Random().Next(100000, 999999);
                        emailVerification.CreatedOn = DateTime.UtcNow;
                        emailVerification.Insert();
                        SendMail(PortalSettings, Email, emailVerification.OTP);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
                return false;
            }

            internal static bool ValidateVerificationCode(int PortalID, string Email, string VerificationCode, out string VerificationMessage)
            {
                try
                {
                    EmailVerification emailVerification = EmailVerification.Query("where Email=@0 and PortalID=@1 and OTP=@2", Email, PortalID, int.Parse(VerificationCode)).FirstOrDefault();
                    if (emailVerification == null)
                    {
                        VerificationMessage = "We could not confirm the verification code.";
                        return false;
                    }
                    else if (emailVerification != null && (DateTime.UtcNow - emailVerification.CreatedOn).TotalMinutes > 5)
                    {
                        VerificationMessage = "Verification code expired.";
                        emailVerification.Delete();
                        return false;
                    }
                    else
                    {
                        VerificationMessage = string.Empty;
                        emailVerification.Delete();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    VerificationMessage = ex.Message;
                    return false;
                }
            }

            private static void SendMail(PortalSettings PortalSettings, string Email, int Otp)
            {
                SmtpClient client = null;
                string NotificationEnabled = PortalController.GetPortalSetting("SMTPmode", PortalSettings.PortalId, "h");
                if (!string.IsNullOrEmpty(NotificationEnabled))
                {
                    bool IsGlobal = NotificationEnabled.Equals("h", StringComparison.OrdinalIgnoreCase);
                    SmtpServer SmtpServer = Common.Manager.NotificationManager.GetSMTP(IsGlobal, PortalSettings.PortalId);
                    try
                    {
                        if (SmtpServer != null && !string.IsNullOrEmpty(SmtpServer.Server) && SmtpServer.Port > 0)
                        {
                            client = NotificationFactory.Connect(SmtpServer.Server, SmtpServer.Port, SmtpServer.Authentication, SmtpServer.Username, SmtpServer.Password, SmtpServer.SSL);
                            if (client != null)
                            {
                                string Content = DotNetNuke.Services.Localization.Localization.GetString("EmailVerification", Components.Constants.LocalResourcesFile);
                                MailQueue mail = new MailQueue
                                {
                                    PortalID = PortalSettings.PortalId,
                                    ModuleID = 0,
                                    Subject = "Verify your email address",
                                    Content = new TokenReplace().ReplaceEnvironmentTokens(Content.Replace("[Enter Code]", Otp.ToString())),
                                    SmtpUniqueId = -1,
                                    Status = "Queue",
                                    RetryAttempt = 0,
                                    RetryDateTime = DateTime.UtcNow,
                                    FromName = PortalSettings.UserInfo.DisplayName,
                                    FromEmail = PortalSettings.UserInfo.Email,
                                    ReplyEmail = PortalSettings.UserInfo.Email,
                                    ToEmail = Email,
                                    Attachment = ""
                                };
                                NotificationFactory.SendMail(client, mail);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                    finally
                    {
                        if (client != null)
                        {
                            client.Dispose();
                        }
                    }
                }
            }
        }
    }
}