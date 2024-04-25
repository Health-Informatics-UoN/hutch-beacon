import json

from beacon_omop_worker.beacon_dto.base_dto import BaseDto


class FilteringTerm(BaseDto):
    def __init__(self, type_: str, id_: str, label: str) -> None:
        self.type = type_
        self.id = id_
        self.label = label

    def __repr__(self):
        return json.dumps(self.to_dict())

    def to_dict(self) -> dict:
        return {"type": self.type, "id": self.id, "label": self.label}
