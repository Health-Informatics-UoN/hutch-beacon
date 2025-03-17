import json

from beacon_omop_worker.beacon_dto.base_dto import BaseDto


class ResponseSummary(BaseDto):
    def __init__(self, exists: bool) -> None:
        self.exists = exists

    def __repr__(self):
        return json.dumps(self.to_dict())

    def to_dict(self) -> dict:
        return {"exists": self.exists}
