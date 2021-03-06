# HttpClient.Stub

The purpose of this package is to ease the stubbing of a HttpClient to behave like a external service. push out your testing boundary

[![NuGet version (HttpClient.Stub)](https://img.shields.io/nuget/v/HttpClient.Stub.svg?style=flat-square)](https://www.nuget.org/packages/HttpClient.Stub/)

When testing code that is dependent on external service I would like to be able to push my stubbing / mocking as far back as it will go. This code is a result of that effort.
It's meant to allow you to configure a HttpClient that will behave like the service you are depending on without changing any actual code.

## Usage

This is a example of a behaviour I have constructed for a Foo server, I want to be able to generate this code based on open API definition. But for now:

```CSharp
public static class FooServiceCalls
{
    private static string FooByIdPath(string fooId)
        => $"foos/{batchId}";

    private static Func<HttpRequestMessage, bool> FooByIdId(string fooId)
        => requestMessage => requestMessage.IsGetRequest() &&
                             requestMessage.IsForPath(FooByIdPath(fooId));

    public static DelegateHandlerBehaviour<IEnumerable<FiniteBatch>> FooByIdIdBehaviour(string fooId,
                                                                                        Foo foo)
        => new(FooByIdPath(fooId),
                FooByIdId(fooId),
                foo);
}
```

Configuring the HttpClient with this behaviour like so:

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    ## Listen in on requests that are made to the HttpClient.
    DelegatingHandlerRequestListener listener = new DelegatingHandlerRequestListener();

    var fooByIdBehaviour = FooServiceCalls.FooByIdIdBehaviour("fooId", FooBuilder.From(fooId));
    DelegatingHandlerBuilder delegatingHandlerBuilder = DelegatingHandlerBuilder.Create
                                                                                .With(listener)
                                                                                .With(fooByIdBehaviour);

    services.AddHttpClient("named_httpClient")
            .AddHttpMessageHandler(() => delegatingHandlerBuilder);
}
```