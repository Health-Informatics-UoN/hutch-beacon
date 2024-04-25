from typing import List
from beacon_omop_worker.beacon_dto.filtering_term import FilteringTerm
from beacon_omop_worker.db_manager import SyncDBManager
from beacon_omop_worker.entities import (
    ConditionOccurrence,
    Person,
    DrugExposure,
    Measurement,
    Observation,
    ProcedureOccurrence,
    Concept,
)
import beacon_omop_worker.config as config
import logging
import pandas as pd
from sqlalchemy import (
    select,
)


class FilterQuerySolver:
    subqueries = list()

    concept_table_map = {
        "Condition": ConditionOccurrence,
        "Ethnicity": Person,
        "Drug": DrugExposure,
        "Gender": Person,
        "Race": Person,
        "Measurement": Measurement,
        "Observation": Observation,
        "Procedure": ProcedureOccurrence,
    }
    concept_time_column_map = {
        "Condition": ConditionOccurrence.condition_start_date,
        "Ethnicity": Person.birth_datetime,
        "Drug": DrugExposure.drug_exposure_start_date,
        "Gender": Person.birth_datetime,
        "Race": Person.birth_datetime,
        "Measurement": Measurement.measurement_date,
        "Observation": Observation.observation_date,
        "Procedure": ProcedureOccurrence.procedure_date,
    }
    numeric_rule_map = {
        "Measurement": Measurement.value_as_number,
        "Observation": Observation.value_as_number,
    }
    boolean_rule_map = {
        "Condition": ConditionOccurrence.condition_concept_id,
        "Ethnicity": Person.ethnicity_concept_id,
        "Drug": DrugExposure.drug_concept_id,
        "Gender": Person.gender_concept_id,
        "Race": Person.race_concept_id,
        "Measurement": Measurement.measurement_concept_id,
        "Observation": Observation.observation_concept_id,
        "Procedure": ProcedureOccurrence.procedure_concept_id,
    }

    def __init__(self, db_manager: SyncDBManager) -> None:
        self.db_manager = db_manager

    def _get_concepts(self):
        concept_query = select(
            Concept.vocabulary_id,
            Concept.concept_code,
            Concept.concept_id,
            Concept.concept_name,
        ).distinct()
        concepts_df = pd.read_sql_query(
            concept_query,
            con=self.db_manager.engine.connect().execution_options(stream_results=True),
        )
        return concepts_df

    def _get_table_concepts(self, query) -> pd.DataFrame:

        table_concepts_df = pd.read_sql_query(
            query, con=self.db_manager.engine.connect()
        )
        return table_concepts_df

    @staticmethod
    def _group_person_concepts(concepts, person_concepts):
        gender_df = (
            concepts.merge(
                person_concepts,
                how="inner",
                left_on=["concept_id"],
                right_on=["gender_concept_id"],
            )
            .drop("race_concept_id", axis=1)
            .drop_duplicates()
        )
        race_df = (
            concepts.merge(
                person_concepts,
                how="inner",
                left_on=["concept_id"],
                right_on=["race_concept_id"],
            )
            .drop("gender_concept_id", axis=1)
            .drop_duplicates()
        )
        filters = list()
        for _, row in gender_df.iterrows():
            filters.append(
                FilteringTerm(
                    id_=f"{row['vocabulary_id']}:{row['concept_code']}",
                    label=row["concept_name"],
                    type_="ontology",
                )
            )
        for _, row in race_df.iterrows():
            filters.append(
                FilteringTerm(
                    id_=f"{row['vocabulary_id']}:{row['concept_code']}",
                    label=row["concept_name"],
                    type_="ontology",
                )
            )
        return filters

    @staticmethod
    def _group_filters(
        concepts: pd.DataFrame, omop_table_df: pd.DataFrame, column: str
    ) -> List[FilteringTerm]:

        filters = list()
        filters_df = concepts.merge(
            omop_table_df,
            how="inner",
            left_on=["concept_id"],
            right_on=[column],
        ).drop_duplicates()
        for _, row in filters_df.iterrows():
            filters.append(
                FilteringTerm(
                    id_=f"{row['vocabulary_id']}:{row['concept_code']}",
                    label=row["concept_name"],
                    type_="ontology",
                )
            )
        return filters

    def solve_concept_filters(self) -> List[FilteringTerm]:

        concepts = self._get_concepts()

        person_query = select(
            Person.race_concept_id, Person.gender_concept_id
        ).distinct()
        person_concepts = self._get_table_concepts(person_query)

        condition_query = select(ConditionOccurrence.condition_concept_id).distinct()
        condition = self._get_table_concepts(condition_query)

        procedure_query = select(ProcedureOccurrence.procedure_concept_id).distinct()
        procedure = self._get_table_concepts(procedure_query)

        measurement_query = select(Measurement.measurement_concept_id).distinct()
        measurement = self._get_table_concepts(measurement_query)

        observation_query = select(Observation.observation_concept_id).distinct()
        observation = self._get_table_concepts(observation_query)

        person_filters = self._group_person_concepts(concepts, person_concepts)
        condition_filters = self._group_filters(
            concepts, condition, "condition_concept_id"
        )
        procedure_filters = self._group_filters(
            concepts, procedure, "procedure_concept_id"
        )
        measurement_filters = self._group_filters(
            concepts, measurement, "measurement_concept_id"
        )
        observations_filters = self._group_filters(
            concepts, observation, "observation_concept_id"
        )
        final_filters = [
            *person_filters,
            *condition_filters,
            *procedure_filters,
            *measurement_filters,
            *observations_filters,
        ]
        return final_filters


def solve_filters(db_manager: SyncDBManager):
    logger = logging.getLogger(config.LOGGER_NAME)
    solver = FilterQuerySolver(db_manager=db_manager)
    try:
        filters = solver.solve_concept_filters()
        logger.info("Successfully extracted filters.")
        return filters
    except Exception as e:
        logger.error(str(e))
