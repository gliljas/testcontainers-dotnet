using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestContainers.Containers.WaitStrategies;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Strategy
{
    public class HostPortWaitStrategyTest
    {

    }

    public class HttpWaitStrategyTest : AbstractWaitStrategyTest<HttpWaitStrategy>
    {
        /**
    * newline sequence indicating end of the HTTP header.
    */
        private static readonly string NEWLINE = "\r\n";

        private static readonly string GOOD_RESPONSE_BODY = "Good Response Body";

        /**
         * Expects that the WaitStrategy returns successfully after receiving an HTTP 200 response from the container.
         */
        [Fact]
        public async Task TestWaitUntilReadyWithSuccess()
        {
            await WaitUntilReadyAndSucceed(CreateShellCommand("200 OK", GOOD_RESPONSE_BODY));
        }

        /**
         * Expects that the WaitStrategy returns successfully after receiving an HTTP 401 response from the container.
         * This 401 response is checked with a lambda using {@link HttpWaitStrategy#forStatusCodeMatching(Predicate)}
         */
        [Fact]
        public async Task TestWaitUntilReadyWithUnauthorizedWithLambda()
        {
            await WaitUntilReadyAndSucceed(StartContainerWithCommand(CreateShellCommand("401 UNAUTHORIZED", GOOD_RESPONSE_BODY),
                CreateHttpWaitStrategy(ref _ready)
                    .ForStatusCodeMatching(it => it >= 200 && it < 300 || it == 401)
            ));
        }

        /**
         * Expects that the WaitStrategy returns successfully after receiving an HTTP 401 response from the container.
         * This 401 response is checked with many status codes using {@link HttpWaitStrategy#forStatusCode(int)}
         */
        [Fact]
        public async Task TestWaitUntilReadyWithManyStatusCodes()
        {
            await WaitUntilReadyAndSucceed(StartContainerWithCommand(CreateShellCommand("401 UNAUTHORIZED", GOOD_RESPONSE_BODY),
                CreateHttpWaitStrategy(ref _ready)
                    .ForStatusCode(300)
                    .ForStatusCode(401)
                    .ForStatusCode(500)
            ));
        }

        /**
         * Expects that the WaitStrategy returns successfully after receiving an HTTP 401 response from the container.
         * This 401 response is checked with with many status codes using {@link HttpWaitStrategy#forStatusCode(int)}
         * and a lambda using {@link HttpWaitStrategy#forStatusCodeMatching(Predicate)}
         */
        [Fact]
        public async Task TestWaitUntilReadyWithManyStatusCodesAndLambda()
        {
            await WaitUntilReadyAndSucceed(StartContainerWithCommand(CreateShellCommand("401 UNAUTHORIZED", GOOD_RESPONSE_BODY),
                CreateHttpWaitStrategy(ref _ready)
                    .ForStatusCode(300)
                    .ForStatusCode(500)
                    .ForStatusCodeMatching(it => it == 401)
            ));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after not receiving any of the
         * error code defined with {@link HttpWaitStrategy#forStatusCode(int)}
         * and {@link HttpWaitStrategy#forStatusCodeMatching(Predicate)}
         */
        [Fact]
        public async Task TestWaitUntilReadyWithTimeoutAndWithManyStatusCodesAndLambda()
        {
            await WaitUntilReadyAndTimeout(StartContainerWithCommand(CreateShellCommand("401 UNAUTHORIZED", GOOD_RESPONSE_BODY),
                CreateHttpWaitStrategy(ref _ready)
                    .ForStatusCode(300)
                    .ForStatusCodeMatching(it => it == 500)
            ));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after not receiving any of the
         * error code defined with {@link HttpWaitStrategy#forStatusCode(int)}
         * and {@link HttpWaitStrategy#forStatusCodeMatching(Predicate)}. Note that a 200 status code should not
         * be considered as a successful return as not explicitly set.
         * Test case for: https://github.com/testcontainers/testcontainers-java/issues/880
         */
        [Fact]
        public async Task TestWaitUntilReadyWithTimeoutAndWithLambdaShouldNotMatchOk()
        {
            await WaitUntilReadyAndTimeout(StartContainerWithCommand(CreateShellCommand("200 OK", GOOD_RESPONSE_BODY),
                CreateHttpWaitStrategy(ref _ready)
                    .ForStatusCodeMatching(it => it >= 300)
            ));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after not receiving an HTTP 200
         * response from the container within the timeout period.
         */
        [Fact]
        public async Task TestWaitUntilReadyWithTimeout()
        {
            await WaitUntilReadyAndTimeout(CreateShellCommand("400 Bad Request", GOOD_RESPONSE_BODY));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after not the expected response body
         * from the container within the timeout period.
         */
        [Fact]
        public async Task TestWaitUntilReadyWithTimeoutAndBadResponseBody()
        {
            await WaitUntilReadyAndTimeout(CreateShellCommand("200 OK", "Bad Response"));
        }


        /**
         * Expects the WaitStrategy probing the right port.
         */
        [Fact]
        public async Task TestWaitUntilReadyWithSpecificPort()
        {
            await WaitUntilReadyAndSucceed(StartContainerWithCommand(
                CreateShellCommand("200 OK", GOOD_RESPONSE_BODY, 9090),
                CreateHttpWaitStrategy(ref _ready)
                    .ForPort(9090),
                7070, 8080, 9090
            ));
        }

        [Fact]
        public async Task TestWaitUntilReadyWithTimoutCausedByReadTimeout()
        {
            await WaitUntilReadyAndTimeout(
                StartContainerWithCommand(CreateShellCommand("0 Connection Refused", GOOD_RESPONSE_BODY, 9090),
                    CreateHttpWaitStrategy(ref _ready).ForPort(9090).WithReadTimeout(TimeSpan.FromMilliseconds(1)),
                    9090
                ));
        }

        /**
         * @param ready the AtomicBoolean on which to indicate success
         * @return the WaitStrategy under test
         */

        protected override HttpWaitStrategy BuildWaitStrategy(ref bool ready)
        {
            return CreateHttpWaitStrategy(ref ready)
                .ForResponsePredicate(s => s.Equals(GOOD_RESPONSE_BODY));
        }

        /**
         * Create a HttpWaitStrategy instance with a waitUntilReady implementation
         *
         * @param ready Indicates that the WaitStrategy has completed waiting successfully.
         * @return the HttpWaitStrategy instance
         */
        private HttpWaitStrategy CreateHttpWaitStrategy(ref bool ready)
        {
            return new TestHttpWaitStrategy(ref ready);
        }

        private class TestHttpWaitStrategy : HttpWaitStrategy
        {
            private bool _ready;

            public TestHttpWaitStrategy(ref bool ready)
            {
                _ready = ready;
            }
            protected async override Task WaitUntilReady(CancellationToken cancellationToken)
            {
                await base.WaitUntilReady(cancellationToken);
                _ready = true;
            }
        }


        private string CreateShellCommand(string header, string responseBody)
        {
            return CreateShellCommand(header, responseBody, 8080);
        }

        private string CreateShellCommand(string header, string responseBody, int port)
        {
            int length = responseBody.Length;// responseBody.getBytes().length;
            return "while true; do { echo -e \"HTTP/1.1 " + header + NEWLINE +
                "Content-Type: text/html" + NEWLINE +
                "Content-Length: " + length + NEWLINE + "\";"
                + " echo \"" + responseBody + "\";} | nc -lp " + port + "; done";
        }
    }
}
