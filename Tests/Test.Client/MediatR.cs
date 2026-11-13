namespace Test.Client
{
    public partial class MediatR
    {
        readonly WebMediatorClient _client = new($"{Settings.WebApiUrl}mediatr", Settings.Client);

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Test]
        [Description("Verifies that a simple ping request returns a valid response with non-empty message data.")]
        public async Task TestPingPong()
        {
            var response = await _client.Send(new Ping { Message = "TEST" });

            Assert.That(response?.Message, Is.Not.Null);
            Assert.That(response?.Message, Is.Not.Empty);
        }

        [Test]
        [Description("Verifies that self-referencing loop requests are safely processed without preserving circular references.")]
        public async Task TestLoop()
        {
            var request = new Looped { Number = 666 };
            request.Child = request;
            var response = await _client.Send(request);

            Assert.That(response?.Child, Is.Null);
            Assert.That(response?.Number, Is.EqualTo(request.Number));
            Assert.That(response?.ReadonlyField, Is.EqualTo(request.ReadonlyField));
            Assert.That(response?.ReadonlyProp, Is.EqualTo(request.ReadonlyProp));
        }
    }
}
