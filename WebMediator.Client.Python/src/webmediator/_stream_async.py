import io
import httpx
from ._utils_async import run_coroutine_sync 
from ._stream import CHUNK_SIZE


class AsyncHttpStreamIO(io.RawIOBase):
    def __init__(self, response: httpx.Response):
        self._response = response
        self._generator = response.aiter_raw(CHUNK_SIZE)
        self._buffer = b""

    def __enter__(self):
        return self
    
    def __exit__(self, exception_type, exception_value, exception_traceback):
        self.close()
        
    async def __aenter__(self):
        return self
    
    async def __aexit__(self, exception_type, exception_value, exception_traceback):
        await self.aclose()

    def readable(self):
        return True
    
    def close(self):
        if not self.closed:
            run_coroutine_sync(self.aclose())
            super().close()
    
    async def aclose(self):
        if not self.closed:
            await self._response.aclose()
            super().close()
    
    def read(self, size=-1):
        return run_coroutine_sync(self.aread(size))
    
    async def aread(self, size=-1):
        if size == 0:
            return b""
        
        # Keep fetching from the generator until we have enough data or it's exhausted
        while size < 0 or len(self._buffer) < size:
            try:
                chunk = await anext(self._generator)
                if not isinstance(chunk, bytes):
                    raise TypeError("Generator must yield bytes objects")
                self._buffer += chunk
            except StopAsyncIteration:
                await self.aclose()
                break # Generator exhausted
            except:
                await self.aclose()
                raise

        if size < 0:
            result = self._buffer
            self._buffer = b""
        else:
            result = self._buffer[:size]
            self._buffer = self._buffer[size:]
            
        return result
    