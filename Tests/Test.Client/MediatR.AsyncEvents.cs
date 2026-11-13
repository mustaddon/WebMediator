using System.Threading;

namespace Test.Client
{
    public partial class MediatR
    {
        [Test]
        [Description("Verifies that event-stream responses publish non-empty event metadata and payload data for generic async events.")]
        public async Task TestAsyncEvents()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var request = new AsyncEvents { Type = "test" };
            var count = 0;
            var lim = 5;

            await foreach (var sse in _client.EventStream(request,
                cancellationToken: cts.Token))
            {
                Assert.That(sse.EventType, Is.Not.Null);
                Assert.That(sse.EventType, Is.Not.Empty);
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data.Type, Is.EqualTo(request.Type));
                Assert.That(sse.Data.Message, Is.Not.Null);
                Assert.That(sse.Data.Message, Is.Not.Empty);

                count++;

                if (count > lim)
                    break;
            }

            Assert.That(count, Is.GreaterThan(lim));
        }

        [Test]
        [Description("Verifies that SSE event payloads preserve the expected event type, message, and data values.")]
        public async Task TestAsyncEventsSse()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var request = new AsyncEventsSse { Type = "test" };
            var count = 0;
            var lim = 5;

            await foreach (var sse in _client.EventStream(request,
                cancellationToken: cts.Token))
            {
                Assert.That(sse.EventType, Is.EqualTo(request.Type));
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data.Type, Is.EqualTo(request.Type));
                Assert.That(sse.Data.Message, Is.Not.Null);
                Assert.That(sse.Data.Message, Is.Not.Empty);

                count++;

                if (count > lim)
                    break;
            }

            Assert.That(count, Is.GreaterThan(lim));
        }

        [Test]
        [Description("Verifies that async event requests return valid event items with matching type and non-empty payload content.")]
        public async Task AsyncEventsRequest()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var request = new AsyncEventsRequest { Type = "test" };
            var count = 0;
            var lim = 5;

            await foreach (var sse in _client.EventStream(request, cancellationToken: cts.Token))
            {
                Assert.That(sse.EventType, Is.Not.Null);
                Assert.That(sse.EventType, Is.Not.Empty);
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data.Type, Is.EqualTo(request.Type));
                Assert.That(sse.Data.Message, Is.Not.Null);
                Assert.That(sse.Data.Message, Is.Not.Empty);

                count++;

                if (count > lim)
                    break;
            }

            Assert.That(count, Is.GreaterThan(lim));
        }

        [Test]
        [Description("Verifies that SSE request variants keep the expected event type and data while streaming multiple updates.")]
        public async Task AsyncEventsRequestSse()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var request = new AsyncEventsRequestSse { Type = "test" };
            var count = 0;
            var lim = 5;

            await foreach (var sse in _client.EventStream(request, cancellationToken: cts.Token))
            {
                Assert.That(sse.EventType, Is.EqualTo(request.Type));
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data.Type, Is.EqualTo(request.Type));
                Assert.That(sse.Data.Message, Is.Not.Null);
                Assert.That(sse.Data.Message, Is.Not.Empty);

                count++;

                if (count > lim)
                    break;
            }

            Assert.That(count, Is.GreaterThan(lim));
        }

        [Test]
        [Description("Verifies that event-stream retries recover after an error and continue delivering valid messages.")]
        public async Task TestAsyncEventsReconnect()
        {
            using var client = new WebMediatorClient(_client.BaseAddress.AbsoluteUri, new ()
            {
                EventStreamOptions = new()
                {
                    ReconnectionDelay = TimeSpan.FromSeconds(1),
                }
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var request = new AsyncEvents { Type = "test", ErrorIndex = 1 };
            var count = 0;
            var lim = 2;

            await foreach (var sse in client.EventStream(request, cancellationToken: cts.Token))
            {
                Assert.That(sse.EventType, Is.Not.Null);
                Assert.That(sse.EventType, Is.Not.Empty);
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data.Type, Is.EqualTo(request.Type));
                Assert.That(sse.Data.Message, Is.Not.Null);
                Assert.That(sse.Data.Message, Is.Not.Empty);

                count++;

                if (count > lim)
                    break;
            }

            Assert.That(count, Is.GreaterThan(lim));
        }


        [Test]
        [Description("Verifies that the reconnection retry limit is enforced and the stream stops once the configured threshold is reached.")]
        public async Task TestAsyncEventsReconnectRetries()
        {
            var settings = new WebMediatorClientSettings()
            {
                EventStreamOptions = new()
                {
                    ReconnectionRetriesLimit = 3,
                    ReconnectionDelay = TimeSpan.Zero,
                }
            };
            using var client = new WebMediatorClient(_client.BaseAddress.AbsoluteUri, settings);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var request = new AsyncEvents { Type = "test", ErrorIndex = 1 };
            var count = -1;

            try
            {
                await foreach (var sse in client.EventStream(request, cancellationToken: cts.Token))
                {
                    if (count > settings.EventStreamOptions.ReconnectionRetriesLimit)
                        break;

                    count++;
                }
            }
            catch  { }

            Assert.That(count, Is.EqualTo(settings.EventStreamOptions.ReconnectionRetriesLimit));
        }
    }
}
