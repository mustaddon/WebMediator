using System.Net.ServerSentEvents;

namespace Test.Client
{
    public partial class MediatR
    {
        [Test]
        public async Task TestAsyncItems()
        {
            var request = new AsyncItems { Count = 5 };
            var response = await _client.Send(request);

            var recived = new List<object>();

            await foreach(var item in response!)
            {
                recived.Add(item);
                Assert.That(item.Message, Is.Not.Null);
                Assert.That(item.Message, Is.Not.Empty);
            }

            Assert.That(request.Count, Is.EqualTo(recived.Count));
        }

        [Test]
        public async Task TestAsyncStructs()
        {
            var request = new AsyncStructs { Count = 5 };
            var response = await _client.Send(request);

            var recived = new List<object>();

            await foreach (var item in response!)
            {
                Assert.That(item, Is.EqualTo(recived.Count));
                recived.Add(item);
            }

            Assert.That(request.Count, Is.EqualTo(recived.Count));
        }

        [Test]
        public async Task TestAsyncSseItem()
        {
            var request = new AsyncStructs<SseItem<string>> { Count = 5, Type = "test" };
            var response = await _client.Send(request);
            var targetType = request.Type ?? new SseItem<int>().EventType;

            var recived = new List<object>();

            await foreach (var sse in response!)
            {
                Assert.That(sse.EventType, Is.EqualTo(targetType));
                Assert.That(sse.EventId, Is.EqualTo(recived.Count.ToString()));
                Assert.That(sse.Data, Is.Not.Null);
                Assert.That(sse.Data, Is.Not.Empty);
                recived.Add(sse);
            }

            Assert.That(request.Count, Is.EqualTo(recived.Count));
        }

        [Test]
        public async Task TestAsyncStream()
        {
            var request = new AsyncStream { Count = 5 };
            var response = await _client.Send(request);

            var recived = new List<object>();

            await foreach (var item in response!)
            {
                recived.Add(item);
                Assert.That(item.Message, Is.Not.Null);
                Assert.That(item.Message, Is.Not.Empty);
            }

            Assert.That(request.Count, Is.EqualTo(recived.Count));
        }
    }
}
