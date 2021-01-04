using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using TestContainers.Core.Containers;

namespace TestContainers.Containers.WaitStrategies
{
    public class HttpWaitStrategy : AbstractWaitStrategy
    {
        private string _path;
        private bool _tlsEnabled;
        private HttpMethod _method;
        private Predicate<string> _responsePredicate;
        private List<int> _statusCodes = new List<int>();
        private Predicate<int> _statusCodePredicate;
        private int? _livenessPort;
        private TimeSpan _readTimeout = TimeSpan.FromSeconds(1);
        private AuthenticationHeaderValue _authorizationHeader;

        public HttpWaitStrategy()
        {

        }

        /// <summary>
        /// Waits for the given status code.
        /// </summary>
        /// <param name="statusCode">an expected status code</param>
        /// <returns>this</returns>
        public HttpWaitStrategy ForStatusCode(int statusCode)
        {
            _statusCodes.Add(statusCode);
            return this;
        }

        public HttpWaitStrategy ForStatusCodeMatching(Predicate<int> statusCodePredicate)
        {
            _statusCodePredicate = statusCodePredicate;
            return this;
        }

        /// <summary>
        /// Waits for the given path.
        /// </summary>
        /// <param name="path">the path to check</param>
        /// <returns>this</returns>
        public HttpWaitStrategy ForPath(string path)
        {
            _path = path;
            return this;
        }

        /// <summary>
        /// Wait for the given port.
        /// </summary>
        /// <param name="port">the given port</param>
        /// <returns>this</returns>
        public HttpWaitStrategy ForPort(int port)
        {
            _livenessPort = port;
            return this;
        }

        /// <summary>
        /// Indicates that the status check should use HTTPS.
        /// </summary>
        /// <returns>this</returns>
        public HttpWaitStrategy UsingTls()
        {
            _tlsEnabled = true;
            return this;
        }

        /// <summary>
        /// Indicates the HTTP method to use (GET by default).
        /// </summary>
        /// <param name="method">the HTTP method</param>
        /// <returns></returns>
        public HttpWaitStrategy WithMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        /// <summary>
        /// Authenticate with HTTP Basic Authorization credentials. 
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        /// <returns></returns>
        public HttpWaitStrategy WithBasicCredentials(string username, string password)
        {
            _authorizationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
               $"{username}:{password}")));
            return this;
        }

        /// <summary>
        /// Set the HTTP connection's read timeout.
        /// </summary>
        /// <param name="timeout">the timeout (minimum 1 millisecond)</param>
        /// <returns>this</returns>
        public HttpWaitStrategy WithReadTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < 1)
            {
                throw new ArgumentOutOfRangeException("you cannot specify a value smaller than 1 ms");
            }
            _readTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Waits for the response to pass the given predicate
        /// </summary>
        /// <param name="responsePredicate">The predicate to test the response against</param>
        /// <returns>this</returns>
        public HttpWaitStrategy ForResponsePredicate(Predicate<string> responsePredicate)
        {
            _responsePredicate = responsePredicate;
            return this;
        }

        protected override async Task WaitUntilReady(CancellationToken cancellationToken)
        {
            var containerName = _waitStrategyTarget.ContainerInfo.Name;

            var livenessCheckPort = -1;

            if (_livenessPort.HasValue)
            {
                livenessCheckPort = await _waitStrategyTarget.GetMappedPort(_livenessPort.Value, cancellationToken);
            }
            else
            {
                var ports = await GetLivenessCheckPorts(cancellationToken);
                if (ports != null && ports.Any())
                {
                    livenessCheckPort = ports.First();
                }
                else
                {

                }
            }

            //final Integer livenessCheckPort = livenessPort.map(waitStrategyTarget::getMappedPort).orElseGet(()-> {
            //    final Set<Integer> livenessCheckPorts = getLivenessCheckPorts();
            //    if (livenessCheckPorts == null || livenessCheckPorts.isEmpty())
            //    {
            //        log.warn("{}: No exposed ports or mapped ports - cannot wait for status", containerName);
            //        return -1;
            //    }
            //    return livenessCheckPorts.iterator().next();
            //});

            if (-1 == livenessCheckPort)
            {
                return;
            }
            //int livenessCheckPort = 0;
            Uri uri = BuildLivenessUri(livenessCheckPort);//.toString();

            //log.info("{}: Waiting for {} seconds for URL: {}", containerName, startupTimeout.getSeconds(), uri);

            // try to connect to the URL
            try
            {
                var p = Policy.TimeoutAsync(30).WrapAsync(
                  Policy.Handle<Exception>()
                    .WaitAndRetryForeverAsync(attempt => TimeSpan.FromSeconds(1)));

                await p.ExecuteAsync(async (token) =>
                {

                    var request = new HttpRequestMessage(_method, uri);
                    //TODO:
                    // connection.setReadTimeout(Math.toIntExact(readTimeout.toMillis()));

                    // authenticate
                    if (_authorizationHeader != null)
                    {
                        request.Headers.Authorization = _authorizationHeader;
                    }

                    var response = await new HttpClient().SendAsync(request, token);

                    //log.trace("Get response code {}", connection.getResponseCode());

                    // Choose the statusCodePredicate strategy depending on what we defined.
                    Predicate<int> predicate;
                    if (!_statusCodes.Any() && _statusCodePredicate == null)
                    {
                        // We have no status code and no predicate so we expect a 200 OK response code
                        predicate = (status) => status == 200;
                    }
                    else if (_statusCodes.Any() && _statusCodePredicate == null)
                    {
                        // We use the default status predicate checker when we only have status codes
                        predicate = (status) => _statusCodes.Contains(status);
                    }
                    else if (!_statusCodes.Any())
                    {
                        // We only have a predicate
                        predicate = _statusCodePredicate;
                    }
                    else
                    {
                        // We have both predicate and status code
                        predicate = (status) => _statusCodePredicate(status) || _statusCodes.Contains(status);
                    }
                    if (!predicate((int)response.StatusCode))
                    {
                        // throw new RuntimeException(String.format("HTTP response code was: %s",
                        //    connection.getResponseCode()));
                    }

                    if (_responsePredicate != null)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();

                        //log.trace("Get response {}", responseBody);

                        if (!_responsePredicate(responseBody))
                        {
                            throw new RuntimeException(string.Format("Response: %s did not match predicate",
                                responseBody));
                        }
                    }

                }, cancellationToken);
            }
            catch (TimeoutException)
            {
                //throw new ContainerLaunchException(string.Format(
                //    "Timed out waiting for URL to be accessible (%s should return HTTP %s)", uri, !_statusCodes.Any() ?
                //        HttpStatusCode.OK : _statusCodes));
            }
        }

        /// <summary>
        /// Build the URI on which to check if the container is ready.
        /// </summary>
        /// <param name="livenessCheckPort">the liveness port</param>
        /// <returns>the liveness URI</returns>
        private Uri BuildLivenessUri(int livenessCheckPort)
        {
            var scheme = (_tlsEnabled ? "https" : "http") + "://";
            var host =_waitStrategyTarget.Host;

            return new UriBuilder(scheme,host,livenessCheckPort,_path).Uri;
        }
    }
}
