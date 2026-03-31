from ._client import *
from ._client_async import *
from ._client_base import *
from ._stream import *
from ._stream_async import *

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