namespace Test.Client
{
    public partial class Sapi : IDisposable
    {
        readonly WebMediatorClient _client = new($"{Settings.WebApiUrl}sapi");

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }


        [Test]
        [Description("Verifies that a string payload is round-tripped without modification through the SAPI endpoint.")]
        public async Task TestString()
        {
            var request = "test";
            var response = await _client.Send<string>(request);

            Assert.That(response, Is.EqualTo(request));
        }

        [Test]
        [Description("Verifies that an integer payload is delivered back unchanged through the SAPI endpoint.")]
        public async Task TestInt()
        {
            var request = 555;
            var response = await _client.Send<int>(request);

            Assert.That(response, Is.EqualTo(request));
        }

        [Test]
        [Description("Verifies that nullable values round-trip correctly and null values remain null across the SAPI endpoint.")]
        public async Task TestNullable()
        {
            var request = (long?)777;
            var response = await _client.Send<long?>(request);
            var responseNull = await _client.Send<long?, long?>(null);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.EqualTo(request));
                Assert.That(responseNull, Is.Null);
            });
        }

        [Test]
        [Description("Verifies that DateTime values are serialized and deserialized without losing their expected value.")]
        public async Task TestDateTime()
        {
            var request = DateTime.Now;
            var response = await _client.Send<DateTime>(request);

            Assert.That(response, Is.EqualTo(request));
        }
    }
}