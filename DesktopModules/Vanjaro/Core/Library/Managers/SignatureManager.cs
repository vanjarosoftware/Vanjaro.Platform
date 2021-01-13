using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.UserRequest;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class SignatureManager
        {
            public static string Sign(string message, string privateKey)
            {
                //// The array to store the signed message in bytes
                byte[] signedBytes;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    //// Write the message to a byte array using UTF8 as the encoding.
                    var encoder = new UTF8Encoding();
                    byte[] originalData = encoder.GetBytes(message);

                    try
                    {
                        //// Import the private key used for signing the message
                        rsa.FromXmlString(privateKey);

                        //// Sign the data, using SHA512 as the hashing algorithm 
                        signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA512"));
                    }
                    catch (CryptographicException e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                    finally
                    {
                        //// Set the keycontainer to be cleared when rsa is garbage collected.
                        rsa.PersistKeyInCsp = false;
                    }
                }
                //// Convert the a base64 string before returning
                return Convert.ToBase64String(signedBytes);
            }

            public static bool Verify(string originalMessage, string signedMessage, string publicKey)
            {
                bool success = false;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var encoder = new UTF8Encoding();
                    byte[] bytesToVerify = encoder.GetBytes(originalMessage);
                    byte[] signedBytes = Convert.FromBase64String(signedMessage);
                    try
                    {
                        rsa.FromXmlString(publicKey);

                        SHA512Managed Hash = new SHA512Managed();

                        byte[] hashedData = Hash.ComputeHash(signedBytes);

                        success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA512"), signedBytes);
                    }
                    catch (CryptographicException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        rsa.PersistKeyInCsp = false;
                    }
                }
                return success;
            }
        }
    }
}