using System;
using System.Collections.Generic;
using WebMediator.Client;

namespace Test
{
    public class Settings
    {
        public static readonly string WebApiUrl = "https://localhost:7263/";
        public static readonly string TempPath = @".\_tmp\";

        public static readonly WebMediatorClientSettings Client = new WebMediatorClientSettings()
        {
            HttpHeaders = new() {
                { "sapi-test", new [] { "test_value" } },
            },

        }.RegisterTypes(GetDeserializerTypes());


        static IEnumerable<Type> GetDeserializerTypes()
        {
            return [];

            //return typeof(Ping).Assembly.GetTypes()
            //    .Where(x => typeof(MediatR.IBaseRequest).IsAssignableFrom(x));
        }
    }
}
