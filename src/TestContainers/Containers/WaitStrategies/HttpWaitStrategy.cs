using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private string _username;
        private List<int> _statusCodes;
        private Predicate<int> _statusCodePredicate;

        public HttpWaitStrategy()
        {

        }

        public HttpWaitStrategy ForPath(string path)
        {
            throw new NotImplementedException();
        }

        public HttpWaitStrategy UsingTls()
        {
            throw new NotImplementedException();
        }

        protected override async Task WaitUntilReady(CancellationToken cancellationToken)
        {
            var containerName = _waitStrategyTarget.GetContainerInfo().Name;

            //final Integer livenessCheckPort = livenessPort.map(waitStrategyTarget::getMappedPort).orElseGet(()-> {
            //    final Set<Integer> livenessCheckPorts = getLivenessCheckPorts();
            //    if (livenessCheckPorts == null || livenessCheckPorts.isEmpty())
            //    {
            //        log.warn("{}: No exposed ports or mapped ports - cannot wait for status", containerName);
            //        return -1;
            //    }
            //    return livenessCheckPorts.iterator().next();
            //});

            //if (null == livenessCheckPort || -1 == livenessCheckPort)
            //{
            //    return;
            //}
            int livenessCheckPort = 0;
            Uri uri = await BuildLivenessUri(livenessCheckPort);//.toString();

            //log.info("{}: Waiting for {} seconds for URL: {}", containerName, startupTimeout.getSeconds(), uri);

            // try to connect to the URL
            try
            {
                var p = Policy.TimeoutAsync(30).WrapAsync(
                  Policy.Handle<Exception>()
                    .WaitAndRetryForeverAsync(attempt => TimeSpan.FromSeconds(1), null));

                var result = await p.ExecuteAndCaptureAsync(async token =>
                {

                    var request = new HttpRequestMessage(_method, uri);

                    // connection.setReadTimeout(Math.toIntExact(readTimeout.toMillis()));

                    // authenticate
                    if (!string.IsNullOrEmpty(_username))
                    {
                        //request.Headers.Authorization=
                        //connection.setRequestProperty(HEADER_AUTHORIZATION, buildAuthString(username, password));
                        //connection.setUseCaches(false);
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
                    if (!predicate(response.StatusCode))
                    {
                        // throw new RuntimeException(String.format("HTTP response code was: %s",
                        //    connection.getResponseCode()));
                    }

                    if (_responsePredicate != null)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync(token);

                        //log.trace("Get response {}", responseBody);

                        if (!_responsePredicate(responseBody))
                        {
                            throw new RuntimeException(string.Format("Response: %s did not match predicate",
                                responseBody));
                        }
                    }

                });
            }
            catch (TimeoutException e)
            {
                throw new ContainerLaunchException(string.Format(
                    "Timed out waiting for URL to be accessible (%s should return HTTP %s)", uri, statusCodes.isEmpty() ?
                        HttpURLConnection.HTTP_OK : statusCodes));
            }
        }

        /**
     * Build the URI on which to check if the container is ready.
     *
     * @param livenessCheckPort the liveness port
     * @return the liveness URI
     */
        private async Task<Uri> BuildLivenessUri(int livenessCheckPort)
        {
            var scheme = (_tlsEnabled ? "https" : "http") + "://";
            var host = await _waitStrategyTarget.GetHost();

            //string portSuffix;
            //if ((_tlsEnabled && 443 == livenessCheckPort) || (!_tlsEnabled && 80 == livenessCheckPort))
            //{
            //    portSuffix = "";
            //}
            //else
            //{
            //    portSuffix = $":{livenessCheckPort}";
            //}

            return new UriBuilder(scheme,host,livenessCheckPort,_path).Uri;
        }
    }
}
