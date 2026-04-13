# webmediator [![PyPI version](https://badge.fury.io/py/webmediator.svg)](https://pypi.org/project/webmediator)
Python sync/async client for the WebMediator API.

```
pip install webmediator
```

## Example 1: request/response
```python
import webmediator

client = webmediator.Client('https://localhost:7263/mediator')

response = client.send('Ping', {'message':'EXAMPLE' })
print(response)
```
*Console output:*
```
type: Pong, data: {'message': 'EXAMPLE PONG'}
```

## Example 2: Async request/response
```python
import webmediator
import asyncio

async def main():
    client = webmediator.AsyncClient('http://localhost:5263/mediator')

    response = await client.send('Ping', {'message':'EXAMPLE' })
    print(response)

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())
```
[More code...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python/dev_async.py)


## Example 3: File upload/download
```python
import webmediator

client = webmediator.Client('https://localhost:7263/mediator')

with open('example.txt','rb') as file: 
    client.send('FileUpload', { 'name': file.name, 'content': file })

with client.send('FileDownload', { 'name': file.name }) as response:
    content = response.data.read()
    print(content)
```
[More code...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python/dev.py)


## Example 4: Server-sent events
```python
import webmediator
import asyncio

async def main():
    client = webmediator.AsyncClient('http://localhost:5263/mediator')

    async for sse in client.event_stream('ExampleAsyncEvents'):
        print(sse)

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())
```
[More code...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python/dev_async.py)

