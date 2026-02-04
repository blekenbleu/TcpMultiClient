# TcpMultiClient
 [TCP Server supporting multiple clients on one port](https://elysiatools.com/en/samples/windows-networking-csharp)

Messages from any client are echoed to others...

Before .NET 7, many async IO methods support neither cancellation nor timeouts.  
Get around that by [closing the stream object](https://stackoverflow.com/a/71698875).  
Other references:  
- [M$ TCP overview:&nbsp; Create a TcpListener](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes)
- [HTTP Server using C# TCP Socket](https://medium.com/@antoharyanto/creating-an-http-server-using-tcp-socket-in-c-without-third-party-libraries-for-a-better-a68d2102b1d0)
### `'Content-Type': 'text/event-stream'`
- [Stream updates with server-sent events - web.dev](https://web.dev/articles/eventsource-basics)
- [9.2 Server-sent events - HTML Living Standard](https://html.spec.whatwg.org/multipage/server-sent-events.html)  
- [Sending events from the server - Mmdn_](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#sending_events_from_the_server)
- [Simplest Server Sent Events In .NET (no libraries)](https://dev.to/yrezehi/simplest-server-sent-events-in-net-no-libraries-2a43)
- [Simple .NET 9 server and client](https://github.com/holgarsson/sse_net9)

