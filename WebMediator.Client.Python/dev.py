import src.webmediator as webmediator

mediator = webmediator.Client('https://localhost:7263/mediator')


with mediator.send('Echo') as res0:
    print(res0)

with mediator.send('Ping', {'message':'TEST' }) as res1:
    print(res1)

with open('test.txt') as file: 
    mediator.send('FileUpload', { 'name': file.name, 'content': file })

with mediator.send('FileDownload', { 'name': file.name }) as res3:
    print(res3, res3.data.read())

with mediator.send('FileDownloadWithInfo', { 'name': file.name }) as res4:
    print(res4, res4.data['content'].read())
