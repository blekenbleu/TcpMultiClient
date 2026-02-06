# TcpMultiClient
 [TCP Server supporting multiple clients on one port](https://elysiatools.com/en/samples/windows-networking-csharp)

Messages from any client are echoed to others...
- Hack for HTTP browser compatibility (waits for first line with GET or POST)
	- this delays telnet client welcome message until after newline receipt

This solution is interesting because
- it works for older C# projects (e.g. .NET Framework 4.8)
- unlike servers based on HttpListener or higher level abstractions,  
  this works on Windows without esclated (admin) permissions for browsers on other devices  
  (TCP/IP addresses other than `127.0.0.1`)
- unlike Http servers which expect to disconnect after each page served,  
  this maintains connections, as is wanted for Server-Sent Events (SSE)
- it [maintains a clients list](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=netframework-4.8) for echoing updates to multiple current connections, as wanted for SSE

Before .NET 7, many async IO methods supported neither cancellation nor timeouts.  
Get around that by [closing the stream object](https://stackoverflow.com/a/71698875).  
Other references:  
- [M$ TCP overview:&nbsp; Create a TcpListener](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes)
- [HTTP Server using C# TCP Socket](https://medium.com/@antoharyanto/creating-an-http-server-using-tcp-socket-in-c-without-third-party-libraries-for-a-better-a68d2102b1d0)
- [simple HttpServer using TCPListener](https://github.com/blekenbleu/HttpServer)

### `'Content-Type': 'text/event-stream'`
- [Stream updates with server-sent events - web.dev](https://web.dev/articles/eventsource-basics)
- [9.2 Server-sent events - HTML Living Standard](https://html.spec.whatwg.org/multipage/server-sent-events.html)  
- [Sending events from the server - Mmdn_](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#sending_events_from_the_server)
- [Simplest Server Sent Events In .NET (no libraries)](https://dev.to/yrezehi/simplest-server-sent-events-in-net-no-libraries-2a43)
- [Simple (25 line) .NET 9 server and client](https://github.com/holgarsson/sse_net9)

