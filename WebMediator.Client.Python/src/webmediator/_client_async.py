import asyncio
import typing
import ssl

import httpx
from httpx._types import AuthTypes, HeaderTypes, CookieTypes, CertTypes, ProxyTypes,TimeoutTypes
from httpx._transports import AsyncBaseTransport
from httpx._config import Limits, DEFAULT_LIMITS, DEFAULT_TIMEOUT_CONFIG, DEFAULT_MAX_REDIRECTS
from httpx._client import EventHook

from ._client_base import BaseClient, Response
from ._stream_async import AsyncHttpStreamIO
from ._event_async import event_async
from ._safe_try_async import safe_try


class AsyncClient(BaseClient):
    def __init__(self, endpoint_url: str,
        auth: AuthTypes | None = None,
        headers: HeaderTypes | None = None,
        cookies: CookieTypes | None = None,
        verify: ssl.SSLContext | str | bool = False,
        cert: CertTypes | None = None,
        trust_env: bool = True,
        http1: bool = True,
        http2: bool = False,
        proxy: ProxyTypes | None = None,
        mounts: None | (typing.Mapping[str, AsyncBaseTransport | None]) = None,
        timeout: TimeoutTypes = DEFAULT_TIMEOUT_CONFIG,
        follow_redirects: bool = False,
        limits: Limits = DEFAULT_LIMITS,
        max_redirects: int = DEFAULT_MAX_REDIRECTS,
        event_hooks: None | (typing.Mapping[str, list[EventHook]]) = None,
        transport: AsyncBaseTransport | None = None,
        default_encoding: str | typing.Callable[[bytes], str] = "utf-8",
        response_stream_as_bytes = False
    ):
        super().__init__(endpoint_url)
        
        self._response_stream_as_bytes = response_stream_as_bytes
        self._client = httpx.AsyncClient(
            base_url=self._endpoint_url,
            auth=auth,
            headers=headers,
            cookies=cookies,
            verify=verify,
            cert=cert,
            trust_env=trust_env,
            http1=http1,
            http2=http2,
            proxy=proxy,
            mounts=mounts,
            timeout=timeout,
            follow_redirects=follow_redirects,
            limits=limits,
            max_redirects=max_redirects,
            event_hooks=event_hooks,
            transport=transport,
            default_encoding=default_encoding)

    def __enter__(self):
        return self
    
    def __exit__(self, exception_type, exception_value, exception_traceback):
        self.close()

    def close(self):
        self._client.close()

    async def aclose(self):
        await self._client.aclose()

    async def _get_result_stream(self, res: httpx.Response):
        if not self._response_stream_as_bytes:
            return AsyncHttpStreamIO(res)
        
        content = await res.aread()
        await res.aclose()
        return content
        
    async def _get_result(self, res: httpx.Response):
        res.raise_for_status()

        data_type = self._get_data_type(res)
        
        if res.status_code== 204:
            await res.aclose()
            return Response(data_type)
        
        if self._is_event_stream_ctype(res):
            return Response(data_type, event_async(res, lambda sse: self._decode_json(sse.data)))
    
        if not self._is_json_ctype(res):
            streamProp = self._get_data_stream_property(res)
            if streamProp:
                data = self._get_data(res)
                data[streamProp] = await self._get_result_stream(res)
                return Response(data_type,
                    data = data,
                    stream_ref = data[streamProp])
            
            data = await self._get_result_stream(res)
            return Response(data_type,
                data = data,
                stream_ref = data)
        
        await res.aread()
        result = Response(data_type, res.json())
        await res.aclose()
        return result
    
    async def _stream_content_async(self, data):
        it = iter(self._stream_content(data))
        done = object()

        while True:
            try:
                value = await asyncio.to_thread(next, it, done)
            except StopIteration:
                break
            
            if value is done:
                break
            
            yield value

    async def send(self, type: str, data=None):
        return await self._get_result(await self._send(type, data))
    
    async def _send(self, type: str, data=None):
        if data == None:
            return await self._post(type)
        
        if self._is_stream(data):
            return await self._post(type, content=self._stream_content_async(data))
        
        io_prop = self._get_stream_prop(data)

        if io_prop:
            copy = data.copy()
            del copy[io_prop]
            return await self._post(f"{type}?data={self._encode_json(copy)}", 
                    content=self._stream_content_async(data[io_prop]))

        return await self._post(type, json=data)
    
    async def _post(self, url, json = None, content = None):
        req = self._client.build_request('POST', url, json=json, content=content)
        return await self._client.send(req, stream=True)
    
    async def event_stream(self, type: str, data=None, reconnection_delay = 3.0, reconnection_retries_limit: int | None = None):
        reconnections = 0

        while True:
            (res, error) = await safe_try(lambda: self._send(type, data))

            if error is None:
                if not self._is_event_stream_ctype(res):
                    await res.aclose()
                    res.raise_for_status()
                    raise httpx.HTTPError(f"The response does not contain 'text/event-stream'. (RequestURL: {res.url})") 
                
                generator = event_async(res, lambda sse:  self._convert_sse(sse))

                while True:
                    (sse, gerror) = await safe_try(lambda: anext(generator))
                    if gerror is not None: break
                    yield sse

            if reconnection_retries_limit is not None and reconnections >= reconnection_retries_limit:
                raise error if res is None else TypeError(f"The limit on the number of reconnection attempts has been reached. (RequestURL: {res.url})") 

            if reconnection_delay > 0:
                await asyncio.sleep(reconnection_delay) 
            
            reconnections += 1