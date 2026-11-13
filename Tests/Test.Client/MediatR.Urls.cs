using TypeSerialization;

namespace Test.Client
{
    public partial class MediatR
    {
        [Test]
        [Description("Verifies that the URL for a request without payload is generated without a query string.")]
        public void GetUrl_WhenRequestIsNull_UsesTypeEndpointWithoutData()
        {
            var url = _client.GetUrl(typeof(Ping), null);

            Assert.That(url, Is.EqualTo($"{_client.BaseAddress}{typeof(Ping).Serialize()}"));
        }

        [Test]
        [Description("Verifies that an empty object is serialized to escaped JSON and appended to the query string.")]
        public void GetUrl_WhenRequestIsEmptyObject_EncodesJsonPayload()
        {
            var url = _client.GetUrl(typeof(AsyncEventsRequest), new AsyncEventsRequest { });

            Assert.That(url, Is.EqualTo($"{_client.BaseAddress}{typeof(AsyncEventsRequest).Serialize()}?data=%7B%7D"));
        }

        [Test]
        [Description("Verifies that requests with a stream property are not allowed to generate a URL.")]
        public void GetUrl_WhenRequestContainsStream_ThrowsInvalidOperationException()
        {
            var request = new FileUpload
            {
                Name = "file.txt",
                Content = new MemoryStream()
            };

            Assert.That(() => _client.GetUrl(typeof(FileUpload), request),
                Throws.InvalidOperationException.With.Message.Contains("stream"));
        }

        [Test]
        [Description("Verifies that URL generation produces the expected endpoints and serialized query payloads across supported request shapes.")]
        public async Task TestUrl()
        {
            Assert.That(_client.GetUrl<Ping>(),
                Is.EqualTo($"{_client.BaseAddress}{typeof(Ping).Serialize()}"));

            Assert.That(_client.GetUrl<int>(),
                Is.EqualTo($"{_client.BaseAddress}{typeof(int).Serialize()}?data=0"));

            Assert.That(_client.GetUrl(123),
                Is.EqualTo($"{_client.BaseAddress}{typeof(int).Serialize()}?data=123"));

            Assert.That(_client.GetUrl(typeof(int), 123),
                Is.EqualTo($"{_client.BaseAddress}{typeof(int).Serialize()}?data=123"));

            Assert.That(_client.GetUrl<string>(),
                Is.EqualTo($"{_client.BaseAddress}{typeof(string).Serialize()}"));

            Assert.That(_client.GetUrl(new Ping { }), 
                Is.EqualTo($"{_client.BaseAddress}{typeof(Ping).Serialize()}?data=%7B%7D"));

            Assert.That(_client.GetUrl(new AsyncEvents { }), 
                Is.EqualTo($"{_client.BaseAddress}{typeof(AsyncEvents).Serialize()}?data=%7B%7D"));

            Assert.That(_client.GetUrl(new AsyncEventsRequest { }),
                Is.EqualTo($"{_client.BaseAddress}{typeof(AsyncEventsRequest).Serialize()}?data=%7B%7D"));
        }
    }
}
