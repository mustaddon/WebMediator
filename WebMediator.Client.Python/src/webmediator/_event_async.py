import httpx
from typing import Callable
from ._sse import ServerSentEvent

async def event_async(response: httpx.Response, converter: Callable[[ServerSentEvent], any] = None):
    try:
        last_event_id = None
        sse = ServerSentEvent()
        async for line in response.aiter_lines():
            if line.startswith('event: '):
                sse.type = line[7:]
            elif line.startswith('data: '):
                sse.data = line[6:]
            elif line.startswith('id: '):
                sse.last_event_id = last_event_id = line[4:]
            elif line == '' and sse.data is not None:
                yield sse if converter is None else converter(sse)
                sse = ServerSentEvent(id=last_event_id)
            
    finally:
        await response.aclose()