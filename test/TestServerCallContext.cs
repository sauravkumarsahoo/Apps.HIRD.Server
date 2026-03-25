using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace HwInfoDisplayTests
{
    /// <summary>
    /// Minimal in-process stub for <see cref="ServerCallContext"/> used in unit tests.
    /// Implements every abstract member with sensible defaults so that production code
    /// that inspects the context (e.g. to extract peer information) follows the same
    /// non-null code paths as at runtime.
    /// </summary>
    internal sealed class TestServerCallContext : ServerCallContext
    {
        private readonly CancellationToken _cancellationToken;

        private TestServerCallContext(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public static TestServerCallContext Create(CancellationToken cancellationToken = default)
            => new TestServerCallContext(cancellationToken);

        protected override string MethodCore => "/hird.SensorService/GetComputerInfo";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "ipv4:127.0.0.1:50051";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => _cancellationToken;
        protected override Metadata ResponseTrailersCore => new Metadata();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore
            => new AuthContext(string.Empty, new Dictionary<string, List<AuthProperty>>());

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
            => throw new NotSupportedException();

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
            => Task.CompletedTask;
    }
}
