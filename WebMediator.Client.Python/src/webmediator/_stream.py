import io
import httpx

CHUNK_SIZE = 4096

class HttpStreamIO(io.RawIOBase):
    def __init__(self, response: httpx.Response):
        self._response = response
        self._generator = response.iter_raw(CHUNK_SIZE)
        self._buffer = b""

    def __enter__(self):
        return self
    
    def __exit__(self, exception_type, exception_value, exception_traceback):
        self.close()

    def readable(self):
        return True

    def close(self):
        if not self.closed:
            self._response.close()
            super().close()

    def read(self, size=-1):
        if size == 0:
            return b""
        
        # Keep fetching from the generator until we have enough data or it's exhausted
        while size < 0 or len(self._buffer) < size:
            try:
                chunk = next(self._generator)
                if not isinstance(chunk, bytes):
                    raise TypeError("Generator must yield bytes objects")
                self._buffer += chunk
            except StopIteration:
                self.close()
                break # Generator exhausted
            except:
                self.close()
                raise

        if size < 0:
            result = self._buffer
            self._buffer = b""
        else:
            result = self._buffer[:size]
            self._buffer = self._buffer[size:]
            
        return result