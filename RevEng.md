[*back*](README.md#content-type-textevent-stream)  
## TCP Server-Sent Events, reverse engineered
- [Chrome developer tool](https://stackoverflow.com/a/46075339) does not handle TCP-level errors
- I use [MSYS2 `telnet.exe`](https://packages.msys2.org/packages/inetutils?variant=x86_64) for TCP-level transactions
- then, disable `await stream.WriteAsync(welcomeBytes...)` in `Program.cs` to capture Chrome browser connection:
```
GET /index.html HTTP/1.1
Host: localhost:8081
Connection: keep-alive

```
### paste that from `telnet localhost 5000` to [SSE-Demo](https://github.com/blekenbleu/sse_net9) to capture:
```
HTTP/1.1 200 OK
Content-Length: 1425
Content-Type: text/html
Date: Sat, 07 Feb 2026 09:53:09 GMT
Server: Kestrel
Accept-Ranges: bytes
ETag: "1dc980e738f2a11"
Last-Modified: Sat, 07 Feb 2026 08:47:55 GMT

<!DOCTYPE html>
<html>
<head>
    <title>Minimal SSE Demo - .NET 9</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        #messages {
            border: 1px solid #ccc;
            padding: 10px;
            height: 300px;
            overflow-y: auto;
            background: #f9f9f9;
        }
        .message { padding: 5px; border-bottom: 1px solid #eee; }
        .status { font-weight: bold; margin: 10px 0; }
    </style>
</head>
<body>
    <h1>Minimal Server-Sent Events Demo</h1>
    <div class="status">Status: <span id="status">Connecting...</span></div>
    <div id="messages"></div>

    <script>
        const source = new EventSource('/sse');
        const messages = document.getElementById('messages');
        const status = document.getElementById('status');

        source.onmessage = (e) => {
            const div = document.createElement('div');
            div.className = 'message';
            div.textContent = e.data;
            messages.insertBefore(div, messages.firstChild);
        };

        source.onopen = () => {
            status.textContent = 'Connected';
            status.style.color = 'green';
        };

        source.onerror = () => {
            status.textContent = 'Disconnected';
            status.style.color = 'red';
        };
    </script>
</body>
</html>
```

### modify `Progam.cs` to send that response to Chrome browser
- capture Chrome browser (JavaScript `/sse`) response to that:
