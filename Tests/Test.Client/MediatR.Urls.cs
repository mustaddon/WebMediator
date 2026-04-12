using TypeSerialization;

namespace Test.Client
{
    public partial class MediatR
    {
        [Test]
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
