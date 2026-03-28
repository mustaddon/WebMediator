global using System;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Net;
global using System.Net.Http.Headers;
global using TypeSerialization;
global using WebMediator.Client.Extensions;

#if !NETSTANDARD2_0
global using System.Net.Http.Json;
#endif