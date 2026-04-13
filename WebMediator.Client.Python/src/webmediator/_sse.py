class ServerSentEvent:
    def __init__(self, type: str = "message", data = None, id: str | None = None):
        self.type = type
        self.data = data
        self.last_event_id = id

    def __str__(self):
        if self.last_event_id is None:
            return f"type: {self.type}, data: {self.data}"
        
        return f"type: {self.type}, last_event_id: {self.last_event_id}, data: {self.data}"
        