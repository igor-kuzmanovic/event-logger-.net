﻿using Helpers;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using WCFServiceCommon;

namespace WCFService
{
    internal class WCFService : IWCFService
    {
        public static SecureString PrivateKey { get; set; }

        public byte[] CheckIn()
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            EventLogger.AuthenticationSuccess(SecurityHelper.GetName(certificate));

            return RSAEncrypter.Encrypt(StringConverter.ToString(PrivateKey), certificate);
        }

        public void Add(string content)
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            if (!RoleBasedAccessControl.UserHasPermission(certificate, Permissions.Add))
            {
                EventLogger.AuthorizationFailure(SecurityHelper.GetName(certificate), "Add", Permissions.Add.ToString());

                throw new FaultException("Unauthorized");
            }

            EventLogger.AuthorizationSuccess(SecurityHelper.GetName(certificate), "Add");

            DatabaseHelper.Add(certificate.SerialNumber, content);
        }

        public bool Update(int entryID, string content)
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            if (!RoleBasedAccessControl.UserHasPermission(certificate, Permissions.Update))
            {
                EventLogger.AuthorizationFailure(SecurityHelper.GetName(certificate), "Update", Permissions.Update.ToString());
                EventLogger.IncreaseAttemps(entryID);

                throw new FaultException("Unauthorized");
            }

            EventLogger.AuthorizationSuccess(SecurityHelper.GetName(certificate), "Update");

            return DatabaseHelper.Update(entryID, certificate.SerialNumber, content);
        }

        public bool Delete(int entryID)
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            if (!RoleBasedAccessControl.UserHasPermission(certificate, Permissions.Delete))
            {
                EventLogger.AuthorizationFailure(SecurityHelper.GetName(certificate), "Delete", Permissions.Delete.ToString());
                EventLogger.IncreaseAttemps(entryID);

                throw new FaultException("Unauthorized");
            }

            EventLogger.AuthorizationSuccess(SecurityHelper.GetName(certificate), "Delete");

            return DatabaseHelper.Delete(entryID);
        }

        public EventEntry Read(int entryID, byte[] key)
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            if (StringConverter.ToString(key) != StringConverter.ToString(PrivateKey))
            {
                EventLogger.AuthorizationFailure(SecurityHelper.GetName(certificate), "Read");

                throw new FaultException("Unauthorized");
            }

            EventLogger.AuthorizationSuccess(SecurityHelper.GetName(certificate), "Read");

            return DatabaseHelper.Read(entryID);
        }

        public HashSet<EventEntry> ReadAll(byte[] key)
        {
            X509Certificate2 certificate = SecurityHelper.GetCertificate(OperationContext.Current);

            if (StringConverter.ToString(key) != StringConverter.ToString(PrivateKey))
            {
                EventLogger.AuthorizationFailure(SecurityHelper.GetName(certificate), "ReadAll");

                throw new FaultException("Unauthorized");
            }

            EventLogger.AuthorizationSuccess(SecurityHelper.GetName(certificate), "ReadAll");

            return DatabaseHelper.ReadAll();
        }

        public byte[] ReadFile()
        {
            return DatabaseHelper.ReadFile();
        }
    }
}
