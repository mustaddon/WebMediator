from ._client import Client
from ._client_async import AsyncClient
from ._client_base import Response
from ._stream import HttpStreamIO
from ._stream_async import AsyncHttpStreamIO

__all__ = [
    "Client",
    "HttpStreamIO",
    "AsyncClient",
    "AsyncHttpStreamIO",
    "Response"
]

__locals = locals()
for __name in __all__:
    if not __name.startswith("__"):
        setattr(__locals[__name], "__module__", "webmediator")  # noqa