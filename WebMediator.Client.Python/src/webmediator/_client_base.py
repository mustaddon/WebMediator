import io
import json
from urllib.parse import quote
import base64
import httpx
    
from ._stream import CHUNK_SIZE, HttpStreamIO
from ._stream_async import AsyncHttpStreamIO

class BaseClient:
    def __init__(self, endpoint_url: str):
        self._endpoint_url = endpoint_url if endpoint_url[-1] == '/' else endpoint_url+'/'

    def _encode_json(self, obj):
        return quote(json.dumps(obj), safe='')
    
    def _is_stream(self, obj):
        return isinstance(obj, io.IOBase) or isinstance(obj, bytes)

    def _get_stream_prop(self, obj):
        if isinstance(obj, dict):
            for key, value in obj.items():
                if value != None and self._is_stream(value):
                    return key
        return None

    def _stream_content(self, data):
        if isinstance(data, bytes):
            yield data
            return
        
        if isinstance(data, io.TextIOBase):
            while chunk := data.read(CHUNK_SIZE):
                yield chunk.encode(data.encoding)
            return    
            
        while chunk := data.read(CHUNK_SIZE):
            yield chunk

    def _is_json_ctype(self, res: httpx.Response) -> bool:
        ctype = res.headers.get('content-type')
        return ctype != None and ctype.find('application/json') >= 0

    def _get_data(self, res: httpx.Response):
        return json.loads(base64.b64decode(res.headers.get('data')))

    def _get_data_type(self, res: httpx.Response) -> str | None:
        return res.headers.get('data-type')
    
    def _get_data_stream_property(self, res: httpx.Response) -> str | None:
        return res.headers.get('data-stream-property')
    
    def get_link(self, type: str, data=None):
        if data == None:
            return self._endpoint_url+type

        if self._is_stream(data) or self._get_stream_prop(data):
            raise TypeError("The data contains stream property (IOBase or bytes) and cannot be presented as a link.")

        return f"{self._endpoint_url}{type}?data={self._encode_json(data)}"
    
 

class Response:
    def __init__(self,
        type: str, 
        data = None, 
        stream_ref : HttpStreamIO | AsyncHttpStreamIO | None = None
    ):
        self.type = type
        self.data = data
        self.__stream_ref = stream_ref if isinstance(stream_ref, (HttpStreamIO, AsyncHttpStreamIO)) else None

    def __enter__(self):
        return self
    
    def __exit__(self, exception_type, exception_value, exception_traceback):
        self.close()

    async def __aenter__(self):
        return self
    
    async def __aexit__(self, exception_type, exception_value, exception_traceback):
        await self.aclose()

    def __str__(self):
        return f"type: {self.type}, data: {self.data}"
    
    def close(self):
        if self.__stream_ref != None:
            self.__stream_ref.close()
            self.__stream_ref = None
            
    async def aclose(self):
        if self.__stream_ref == None:
            return
        
        if isinstance(self.__stream_ref, AsyncHttpStreamIO):
            await self.__stream_ref.aclose()
        else:
            self.__stream_ref.close()
        
        self.__stream_ref = None
        