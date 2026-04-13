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


#### Console output:
## type: Pong, data: {'message': 'EXAMPLE PONG'}
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


## Example 4: Async streams
```python
import webmediator
import asyncio

async def main():
    client = webmediator.AsyncClient('http://localhost:5263/mediator')

    response = await client.send('AsyncItemsStream', { 'count': 3 })

    async for item in response.data:
        print(item)

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())



#### Console output:
## {'index': 0, 'text': 'example async item #0'}
## {'index': 1, 'text': 'example async item #1'}
## {'index': 2, 'text': 'example async item #2'}
```
[More code...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python/dev_async.py)



## Example 5: Server-sent events
```python
import webmediator
import asyncio

async def main():
    client = webmediator.AsyncClient('http://localhost:5263/mediator')

    async for sse in client.event_stream('ExampleAsyncEvents'):
        print(sse)

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())



#### Console output:
## type: message, last_event_id: 0, data: #0 example event
## type: message, last_event_id: 1, data: #1 example event
## type: message, last_event_id: 2, data: #2 example event
## type: message, last_event_id: 3, data: #3 example event
## type: message, last_event_id: 4, data: #4 example event
```
[More code...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python/dev_async.py)

