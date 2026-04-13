import src.webmediator as webmediator
import asyncio


async def main():
    mediator = webmediator.AsyncClient('http://localhost:5263/mediator')


    async with await mediator.send('Echo') as res0:
        print(res0)

    async with await mediator.send('Ping', {'message':'TEST' }) as res1:
        print(res1)

    with open('test.txt') as file: 
        await mediator.send('FileUpload', { 'name': file.name, 'content': file })

    async with await mediator.send('FileDownload', { 'name': file.name }) as res3:
        print(res3, await res3.data.aread())

    async with await mediator.send('FileDownloadWithInfo', { 'name': file.name }) as res4:
        print(res4, res4.data['content'].read())

    async for sse in mediator.event_stream('ExampleAsyncEvents'):
        print(sse)

    # async with await mediator.send('AsyncItems', { 'count': 5 }) as res5:
    #     print(res5)
    #     async for value in res5.data:
    #         print(value)
    
    # async for sse in mediator.event_stream('AsyncEventsSse',
    #     data = { 'type': 'test', 'ErrorIndex': 3 }, 
    #     reconnection_delay = 1,
    #     reconnection_retries_limit = 3):
    #     print(sse)

    

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())