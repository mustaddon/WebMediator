import src.webmediator as webmediator

mediator = webmediator.Client('https://localhost:7263/mediator')


with mediator.send('Echo') as res0:
    print(res0)

with mediator.send('Ping', {'Message':'TEST' }) as res1:
    print(res1)

with open('test.txt') as file: 
    with mediator.send('FileUpload', { 'Name': file.name, 'Content': file }) as res2:
        print(res2)

with mediator.send('FileDownload', { 'Path': res2.data }) as res3:
    print(res3, res3.data.read())

with mediator.send('FileDownloadWithInfo', { 'Path': res2.data }) as res4:
    print(res4, res4.data['Content'].read())
