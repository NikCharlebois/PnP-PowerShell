﻿#if !NETSTANDARD2_1
using Microsoft.Graph;
#endif
using SharePointPnP.PowerShell.Commands.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.PowerShell.Commands.Base
{
    /// <summary>
    /// Base class for all the PnP Microsoft Graph related cmdlets
    /// </summary>
    public abstract class PnPGraphCmdlet : BasePSCmdlet
    {
        public String AccessToken
        {
            get
            {
                if (SPOnlineConnection.AuthenticationResult != null)
                {
                    if (SPOnlineConnection.AuthenticationResult.ExpiresOn < DateTimeOffset.Now)
                    {
                        WriteWarning(Resources.MicrosoftGraphOAuthAccessTokenExpired);
                        SPOnlineConnection.AuthenticationResult = null;
                        return null;
                    }
                    else
                    {
                        return (SPOnlineConnection.AuthenticationResult.AccessToken);
                    }
                }
                else if (SPOnlineConnection.CurrentConnection?.AccessToken != null)
                {
                    return SPOnlineConnection.CurrentConnection.AccessToken;
                }
                else
                {
#if !ONPREMISES
                    var token = TokenHandler.AcquireToken("https://graph.microsoft.com", null);
                    if (!string.IsNullOrEmpty(token))
                    {
                        SPOnlineConnection.CurrentConnection.AccessToken = token;
                        return token;
                    }
                    else
                    {
                        ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(Resources.NoAzureADAccessToken), "NO_OAUTH_TOKEN", ErrorCategory.ConnectionError, null));
                        return null;
                    }
#else
                    return null;
#endif
                }
            }
        }

        protected override void BeginProcessing()
        {

            base.BeginProcessing();


#if !NETSTANDARD2_1

            //if (SPOnlineConnection.CurrentConnection != null && SPOnlineConnection.CurrentConnection.ConnectionMethod == Model.ConnectionMethod.GraphDeviceLogin)
            //{
            //    if (string.IsNullOrEmpty(SPOnlineConnection.CurrentConnection.AccessToken))
            //    {
            //        throw new InvalidOperationException(Resources.NoAzureADAccessToken);
            //    }
            //}
            //else
            //{
            //    if (SPOnlineConnection.AuthenticationResult == null ||
            //    String.IsNullOrEmpty(SPOnlineConnection.AuthenticationResult.Token))
            //    {
            //        throw new InvalidOperationException(Resources.NoAzureADAccessToken);
            //    }
            //}
#else
            if (SPOnlineConnection.CurrentConnection != null && (SPOnlineConnection.CurrentConnection.ConnectionMethod == Model.ConnectionMethod.GraphDeviceLogin || SPOnlineConnection.CurrentConnection.ConnectionMethod == Model.ConnectionMethod.AccessToken))
            {
                // Graph Connection
                if (string.IsNullOrEmpty(SPOnlineConnection.CurrentConnection.AccessToken))
                {
                    throw new InvalidOperationException(Resources.NoAzureADAccessToken);
                }
            }
            else
            {
                //Normal connection
                if (SPOnlineConnection.AuthenticationResult == null ||
                string.IsNullOrEmpty(SPOnlineConnection.AuthenticationResult.AccessToken))
                {
                    throw new InvalidOperationException(Resources.NoAzureADAccessToken);
                }
            }
#endif
        }

        protected virtual void ExecuteCmdlet()
        { }

        protected override void ProcessRecord()
        {
            ExecuteCmdlet();
        }
    }
}
