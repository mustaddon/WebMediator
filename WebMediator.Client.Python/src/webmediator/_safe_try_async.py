
from typing import Callable, Awaitable

async def safe_try(factory: Callable[[], Awaitable]):
    try:
        return (await factory(), None)
    except Exception as ex:
        return (None, ex)


