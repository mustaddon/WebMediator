namespace Test.Client
{
    public partial class Sapi
    {
        [Test]
        [Description("Verifies that a stream payload is returned as the original file content after the SAPI round trip.")]
        public async Task TestStream()
        {
            using var request = new MemoryStream(Encoding.UTF8.GetBytes(TestData.FileContent));
            using var response = await _client.Send<Stream>(request);
            Assert.That(response?.ToText(), Is.EqualTo(TestData.FileContent));
        }
    }
}
