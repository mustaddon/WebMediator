import src.webmediator as webmediator
import asyncio


async def main():
    mediator = webmediator.AsyncClient('http://localhost:5263/mediator')


    async with await mediator.send('Echo') as res0:
        print(res0)

    async with await mediator.send('Ping', {'Message':'TEST' }) as res1:
        print(res1)

    with open('test.txt') as file: 
        async with await mediator.send('FileUpload', { 'Name': file.name, 'Content': file }) as res2:
            print(res2)

    async with await mediator.send('FileDownload', { 'Path': res2.data }) as res3:
        print(res3, await res3.data.aread())

    async with await mediator.send('FileDownloadWithInfo', { 'Path': res2.data }) as res4:
        print(res4, res4.data['Content'].read())

    

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())