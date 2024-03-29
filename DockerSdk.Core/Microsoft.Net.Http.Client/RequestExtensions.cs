﻿using System;
using System.Net.Http;

namespace Microsoft.Net.Http.Client
{
    internal static class RequestExtensions
    {
        public static bool IsHttp(this HttpRequestMessage request)
        {
            return string.Equals("http", request.GetSchemeProperty(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHttps(this HttpRequestMessage request)
        {
            return string.Equals("https", request.GetSchemeProperty(), StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetSchemeProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.Scheme");
        }

        public static void SetSchemeProperty(this HttpRequestMessage request, string? scheme)
        {
            request.SetProperty("url.Scheme", scheme);
        }

        public static string? GetHostProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.Host");
        }

        public static void SetHostProperty(this HttpRequestMessage request, string? host)
        {
            request.SetProperty("url.Host", host);
        }

        public static int? GetPortProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<int?>("url.Port");
        }

        public static void SetPortProperty(this HttpRequestMessage request, int? port)
        {
            request.SetProperty("url.Port", port);
        }

        // The host, after considering the proxy
        public static string? GetConnectionHostProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.ConnectionHost");
        }

        // The host, after considering the proxy
        public static void SetConnectionHostProperty(this HttpRequestMessage request, string? host)
        {
            request.SetProperty("url.ConnectionHost", host);
        }

        // The port, after considering the proxy
        public static int? GetConnectionPortProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<int?>("url.ConnectionPort");
        }

        // The port, after considering the proxy
        public static void SetConnectionPortProperty(this HttpRequestMessage request, int? port)
        {
            request.SetProperty("url.ConnectionPort", port);
        }

        public static string? GetPathAndQueryProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.PathAndQuery");
        }

        public static void SetPathAndQueryProperty(this HttpRequestMessage request, string? pathAndQuery)
        {
            request.SetProperty("url.PathAndQuery", pathAndQuery);
        }

        public static string? GetAddressLineProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.AddressLine");
        }

        // TODO: what is this? sometimes set to $"{scheme}://{host}:{port}{pathAndQuery}", other times to pathAndQuery, other times {host}:{port}. is it the part of the HTTP request's first line immediately after the verb?
        public static void SetAddressLineProperty(this HttpRequestMessage request, string? addressLine)
        {
            request.SetProperty("url.AddressLine", addressLine);
        }

        public static T? GetProperty<T>(this HttpRequestMessage request, string key)
        {
#pragma warning disable CS0618 // Type or member is obsolete // TODO: make tests for this and then change it
            if (request.Properties.TryGetValue(key, out var obj))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return (T?)obj;
            }
            return default;
        }

        public static void SetProperty<T>(this HttpRequestMessage request, string key, T? value)
        {
#pragma warning disable CS0618 // Type or member is obsolete // TODO: make tests for this and then change it
            request.Properties[key] = value;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
