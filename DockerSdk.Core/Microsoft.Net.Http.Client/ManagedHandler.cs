using DockerSdk.Core;
using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Net.Http.Client
{
	public sealed class ManagedHandler : HttpMessageHandler
	{
		public delegate Task<Stream> StreamOpener(string host, int port, CancellationToken cancellationToken);
		public delegate Task<Socket> SocketOpener(string host, int port, CancellationToken cancellationToken);

		private ManagedHandler()
		{
			_socketOpener = TCPSocketOpenerAsync;
		}

		private ManagedHandler(StreamOpener opener)
		{
			_streamOpener = opener;
		}

		private ManagedHandler(SocketOpener opener)
		{
			_socketOpener = opener;
		}

		public IWebProxy? Proxy
		{
			get
			{
                _proxy ??= WebRequest.DefaultWebProxy;
				return _proxy;
			}
			set
			{
				_proxy = value;
			}
		}

		public bool UseProxy { get; set; } = true;

		public int MaxAutomaticRedirects { get; set; } = 20;

		public RedirectMode RedirectMode { get; set; } = RedirectMode.NoDowngrade;

        public X509CertificateCollection ClientCertificates { get; set; } = new();

        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; } 
            = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => false; // TODO: what's a good default?


        private readonly StreamOpener? _streamOpener;
		private readonly SocketOpener? _socketOpener;
		private IWebProxy? _proxy;

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
            
            int redirectCount = 0;
            bool retry;

            HttpResponseMessage? response;
            do
            {
                retry = false;

                // Send the request.
                response = await ProcessRequestAsync(request, cancellationToken);

                // If the response has a redirection status code, it meets the configured redirection criteria, and we
                // haven't exceeded the max redirections limit, update the request to the new location and try again.
                if (redirectCount < MaxAutomaticRedirects && IsAllowedRedirectResponse(request, response))
                {
                    redirectCount++;
                    retry = true;
                }

            } while (retry);

            return response;
		}

        // TODO: rename method or move mutations out
		private bool IsAllowedRedirectResponse(HttpRequestMessage request, HttpResponseMessage response)
		{
			// Are redirects enabled?
			if (RedirectMode == RedirectMode.None)
			{
				return false;
			}

			// Only allow codes 301 and 302.
			if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.Moved)
			{
				return false;
			}

			Uri? location = response.Headers.Location;
			if (location == null)
			{
				return false;
			}

			if (!location.IsAbsoluteUri)
			{
				request.RequestUri = location;
				request.SetPathAndQueryProperty(null);
				request.SetAddressLineProperty(null);
				request.Headers.Authorization = null;
				return true;
			}

			// Check if redirect from https to http is allowed
			if (request.IsHttps() && string.Equals("http", location.Scheme, StringComparison.OrdinalIgnoreCase)
				&& RedirectMode == RedirectMode.NoDowngrade)
			{
				return false;
			}

			// Reset fields calculated from the URI.
			request.RequestUri = location;
			request.SetSchemeProperty(null);
			request.Headers.Host = null;
			request.Headers.Authorization = null;
			request.SetHostProperty(null);
			request.SetConnectionHostProperty(null);
			request.SetPortProperty(null);
			request.SetConnectionPortProperty(null);
			request.SetPathAndQueryProperty(null);
			request.SetAddressLineProperty(null);
			return true;
		}

		private async Task<HttpResponseMessage> ProcessRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

            ProcessUrl(request);
            ProcessHostHeader(request);
			request.Headers.ConnectionClose = true; // TODO: Connection re-use is not supported.

			ProxyMode proxyMode = DetermineProxyModeAndAddressLine(request);
			Socket? socket;
			Stream transport;
			try
			{
				if (_socketOpener != null)
				{
					socket = await _socketOpener(request.GetConnectionHostProperty()!, request.GetConnectionPortProperty()!.Value, cancellationToken).ConfigureAwait(false);
					transport = new NetworkStream(socket, true);
				}
				else if (_streamOpener != null)
				{
					socket = null;
					transport = await _streamOpener(request.GetConnectionHostProperty()!, request.GetConnectionPortProperty()!.Value, cancellationToken).ConfigureAwait(false);
				}
                else
                {
                    throw new InvalidOperationException("Expected either the socket opener or the stream opener to be non-null.");
                }
			}
			catch (SocketException sox)
			{
				throw new HttpRequestException("Connection failed", sox);
			}

			if (proxyMode == ProxyMode.Tunnel)
			{
				await TunnelThroughProxyAsync(request, transport, cancellationToken);
			}

			System.Diagnostics.Debug.Assert(!(proxyMode == ProxyMode.Http && request.IsHttps()));

			if (request.IsHttps())
			{
				SslStream sslStream = new SslStream(transport, false, ServerCertificateValidationCallback);
				await sslStream.AuthenticateAsClientAsync(request.GetHostProperty()!, ClientCertificates, SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, false);
				transport = sslStream;
			}

			var bufferedReadStream = new BufferedReadStream(transport, socket);
			var connection = new HttpConnection(bufferedReadStream);
			return await connection.SendAsync(request, cancellationToken);
		}

        // Copies data URL information from the request's URL to the request's Properties dictionary.
		// Data comes from either the request.RequestUri or from the request.Properties
		private static void ProcessUrl(HttpRequestMessage request)
		{
            if (request.RequestUri == null)
                throw new ArgumentException("The request is missing its URL.", nameof(request));
 
			void RequireAbsoluteUri()
			{
				if (!request.RequestUri.IsAbsoluteUri)
					throw new InvalidOperationException("Missing URL scheme");
			}

            // If we don't already have the URL scheme saved, save it now.
			string? scheme = request.GetSchemeProperty();
			if (string.IsNullOrWhiteSpace(scheme))
			{
				RequireAbsoluteUri();
				scheme = request.RequestUri.Scheme;
				request.SetSchemeProperty(scheme);
			}

            // Only accept HTTP/HTTPS.
			if (!request.IsHttp() && !request.IsHttps())
				throw new InvalidOperationException("Only HTTP or HTTPS are supported, not " + request.RequestUri.Scheme);

            // If we don't already have the host saved, save it now.
			string? host = request.GetHostProperty();
			if (string.IsNullOrWhiteSpace(host))
			{
				RequireAbsoluteUri();
				host = request.RequestUri.DnsSafeHost;
				request.SetHostProperty(host);
			}

            // TODO: ???
			string? connectionHost = request.GetConnectionHostProperty();
			if (string.IsNullOrWhiteSpace(connectionHost))
			{
				request.SetConnectionHostProperty(host);
			}

			int? port = request.GetPortProperty();
			if (!port.HasValue)
			{
				RequireAbsoluteUri();
				port = request.RequestUri.Port;
				request.SetPortProperty(port);
			}

            // TODO: ???
            int? connectionPort = request.GetConnectionPortProperty();
			if (!connectionPort.HasValue)
			{
				request.SetConnectionPortProperty(port);
			}

			string? pathAndQuery = request.GetPathAndQueryProperty();
			if (string.IsNullOrWhiteSpace(pathAndQuery))
			{
				if (request.RequestUri.IsAbsoluteUri)
				{
					pathAndQuery = request.RequestUri.PathAndQuery;
				}
				else
				{
					pathAndQuery = Uri.EscapeUriString(request.RequestUri.ToString());
				}
				request.SetPathAndQueryProperty(pathAndQuery);
			}
		}

        /// <summary>
        /// Sets the request's Host header, if it hasn't already been set.
        /// </summary>
        /// <param name="request"></param>
		private static void ProcessHostHeader(HttpRequestMessage request)
		{
			if (string.IsNullOrWhiteSpace(request.Headers.Host))
			{
				string host = request.GetHostProperty()!;
				int port = request.GetPortProperty()!.Value;
				if (host.Contains(':'))
				{
					// IPv6
					host = '[' + host + ']';
				}

				request.Headers.Host = host + ":" + port.ToString(CultureInfo.InvariantCulture);
			}
		}

		private ProxyMode DetermineProxyModeAndAddressLine(HttpRequestMessage request)
		{
            if (request.RequestUri == null)
                throw new ArgumentException("The request is missing its URL.", nameof(request));

			string scheme = request.GetSchemeProperty()!;
			string host = request.GetHostProperty()!;
			int port = request.GetPortProperty()!.Value;
			string pathAndQuery = request.GetPathAndQueryProperty()!;

			string? addressLine = request.GetAddressLineProperty();
			if (string.IsNullOrEmpty(addressLine))
			{
				request.SetAddressLineProperty(pathAndQuery);
			}

			try
			{
				if (!UseProxy || (Proxy == null) || Proxy.IsBypassed(request.RequestUri))
				{
					return ProxyMode.None;
				}
			}
			catch (PlatformNotSupportedException)
			{
				return ProxyMode.None;
			}

            // Get the proxy URL for the desired destination. If the result is null, we don't need a proxy.
			var proxyUri = Proxy.GetProxy(request.RequestUri);
			if (proxyUri == null)
			{
				return ProxyMode.None;
			}

			if (request.IsHttp())
			{
				if (string.IsNullOrEmpty(addressLine))
				{
					addressLine = $"{scheme}://{host}:{port}{pathAndQuery}";
					request.SetAddressLineProperty(addressLine);
				}
				request.SetConnectionHostProperty(proxyUri.DnsSafeHost);
				request.SetConnectionPortProperty(proxyUri.Port);
				return ProxyMode.Http;
			}

			// Tunneling generates a completely seperate request, don't alter the original, just the connection address.
			request.SetConnectionHostProperty(proxyUri.DnsSafeHost);
			request.SetConnectionPortProperty(proxyUri.Port);
			return ProxyMode.Tunnel;
		}

		private static async Task<Socket> TCPSocketOpenerAsync(string host, int port, CancellationToken ct)
		{
			var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
			if (addresses.Length == 0)
			{
				throw new InvalidOperationException($"Could not resolve addresses for {host}.");
			}

			Socket? connectedSocket = null;
			Exception? lastException = null;
			foreach (var address in addresses)
			{
				var s = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					await s.ConnectAsync(address, port, ct).ConfigureAwait(false);
					connectedSocket = s;
					break;
				}
				catch (Exception e)
				{
					s.Dispose();
					lastException = e;
				}
			}

			if (connectedSocket == null)
			{
				throw lastException!;
			}

			return connectedSocket;
		}

		private static async Task TunnelThroughProxyAsync(HttpRequestMessage request, Stream transport, CancellationToken cancellationToken)
		{
            // Send a Connect request:
            // CONNECT server.example.com:80 HTTP / 1.1
            // Host: server.example.com:80
            var connectRequest = new HttpRequestMessage
            {
                Version = new Version(1, 1)
            };

            connectRequest.Headers.ProxyAuthorization = request.Headers.ProxyAuthorization;
			connectRequest.Method = new HttpMethod("CONNECT");
			// TODO: IPv6 hosts
			string authority = request.GetHostProperty() + ":" + request.GetPortProperty()!.Value;
			connectRequest.SetAddressLineProperty(authority);
			connectRequest.Headers.Host = authority;

			var connection = new HttpConnection(new BufferedReadStream(transport, null));
			HttpResponseMessage connectResponse;
			try
			{
                connectResponse = await connection.SendAsync(connectRequest, cancellationToken);
				// TODO:? await connectResponse.Content.LoadIntoBufferAsync(); // Drain any body
				// There's no danger of accidently consuming real response data because the real request hasn't been sent yet.
			}
			catch (Exception ex)
			{
				transport.Dispose();
				throw new HttpRequestException("SSL Tunnel failed to initialize", ex);
			}

			// Listen for a response. Any 2XX is considered success, anything else is considered a failure.
			if ((int)connectResponse.StatusCode is < 200 or >= 300)
			{
				transport.Dispose();
				throw new HttpRequestException("Failed to negotiate the proxy tunnel: " + connectResponse.ToString());
			}
		}

		//
		internal static HttpMessageHandler Create(CommOptions configuration, out Uri uri)
		{
			var innerHandler = CreateCore(configuration, out uri);
			return configuration.Credentials!.GetHandler(innerHandler);
		}

		internal static ManagedHandler CreateCore(CommOptions configuration, out Uri uri)
		{
			uri = configuration.DaemonUrl;

			switch (uri.Scheme.ToLowerInvariant())
			{
				case "npipe":
					if (!OperatingSystem.IsWindows())
						throw new PlatformNotSupportedException("Named pipes are only supported on Windows.");
					return CreateNamedPipeHandler(configuration, ref uri);

				case "tcp":
				case "http":
					var builder = new UriBuilder(uri)
					{
						Scheme = configuration.Credentials!.IsTlsCredentials() ? "https" : "http"
					};
					uri = builder.Uri;
					return new ManagedHandler();

				case "https":
					return new ManagedHandler();

				case "unix":
					return CreateUnixSocketHandler(ref uri);

				default:
					throw new Exception($"Unknown URL scheme {configuration.DaemonUrl.Scheme}");
			}
		}

		private static ManagedHandler CreateUnixSocketHandler(ref Uri uri)
		{
			var pipeString = uri.LocalPath;
			uri = new UriBuilder("http", uri.Segments.Last()).Uri;
			return new ManagedHandler(async (string host, int port, CancellationToken ct) =>
			{
				var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
				await sock.ConnectAsync(new UnixDomainSocketEndPoint(pipeString), ct);
				return sock;
			});
		}

		[SupportedOSPlatform("windows")]
		private static ManagedHandler CreateNamedPipeHandler(CommOptions configuration, ref Uri uri)
		{
			if (configuration.Credentials!.IsTlsCredentials())
			{
				throw new Exception("TLS not supported over npipe");
			}

			var segments = uri.Segments;
			if (segments.Length != 3 || !segments[1].Equals("pipe/", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException($"{configuration.DaemonUrl} is not a valid npipe URI");
			}

			var serverName = uri.Host;
			if (string.Equals(serverName, "localhost", StringComparison.OrdinalIgnoreCase))
			{
				// npipe schemes dont work with npipe://localhost/... and need npipe://./... so fix that for a client here.
				serverName = ".";
			}

			var pipeName = uri.Segments[2];

			uri = new UriBuilder("http", pipeName).Uri;
			return new ManagedHandler(async (host, port, cancellationToken) =>
			{
				int timeout = (int)configuration.DefaultTimeout.TotalMilliseconds;
				var stream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
				var dockerStream = new DockerPipeStream(stream);

				await stream.ConnectAsync(timeout, cancellationToken);
				return dockerStream;
			});
		}
	}
}
