namespace Test.Client
{
    public partial class Sapi : IDisposable
    {

        [Test]
        [Description("Verifies that arrays with null values are correctly reconstructed and retain the expected elements.")]
        public async Task TestArray()
        {
            var request = new int?[] { 555, null, 666 };

            var response = await _client.Send<int?[]>(request);

            Assert.That(response, Is.EquivalentTo(request));
        }

        [Test]
        [Description("Verifies that generic list payloads are round-tripped with the same values and ordering.")]
        public async Task TestList()
        {
            var request = new List<int> { 111, 222, 333 };

            var response = await _client.Send<List<int>>(request);

            Assert.That(response, Is.EquivalentTo(request));
        }

        [Test]
        [Description("Verifies that dictionary payloads keep the same keys and nested array values after serialization.")]
        public async Task TestDictionary()
        {
            var request = new Dictionary<string, int[]> {
                { "key_1", new [] { 1, 11, 111 } },
                { "key_2", new [] { 2, 22, 222 } },
            };

            var response = await _client.Send<Dictionary<string, int[]>>(request);

            Assert.That(response, Is.EquivalentTo(request));
        }
    }
}