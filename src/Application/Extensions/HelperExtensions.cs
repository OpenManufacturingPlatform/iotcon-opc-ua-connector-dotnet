﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Diagnostics;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Schema.Messages;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Extensions
{
    public static class HelperExtensions
    {
        public static string GetBaseEndpointUrl(this Session session)
        {
            var hostname = session.ConfiguredEndpoint.EndpointUrl.Host;
            var port = session.ConfiguredEndpoint.EndpointUrl.Port;
            return $"{Constants.OpcTcpPrefix}{hostname}:{port}/";
        }

        public static string ToValidBaseEndpointUrl(this string endpointUrl)
        {
            try
            {
                var uriBuilder = new UriBuilder(endpointUrl);
                return $"{Constants.OpcTcpPrefix}{uriBuilder.Host}:{uriBuilder.Port}{uriBuilder.Path}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid endpoint url detected: {endpointUrl}", ex.Demystify());
            }
        }

        public static string GetEndpointUrl(this CommandRequest commandRequest)
        {
            return commandRequest.Payload.RequestTarget.EndpointUrl;
        }
    }
}