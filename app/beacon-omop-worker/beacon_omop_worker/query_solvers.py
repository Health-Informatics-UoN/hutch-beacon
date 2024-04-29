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
    Vocabulary,
)
import beacon_omop_worker.config as config
import logging
import pandas as pd
from sqlalchemy import (
    select,
)


class FilterQuerySolver:

    def __init__(self, db_manager: SyncDBManager) -> None:
        self.db_manager = db_manager

    def _get_concepts(self) -> pd.DataFrame:
        """
        Select vocabulary_id, concept_name, concept_id columns from Concept table
        Returns:
        concepts_df (pd.DataFrame): Concept table with relevant columns as a pandas dataframe.
        """
        concept_query = select(
            Concept.vocabulary_id,
            Concept.concept_id,
            Concept.concept_name,
            Concept.concept_code,
        ).distinct()
        concepts_df = pd.read_sql_query(
            concept_query,
            con=self.db_manager.engine.connect().execution_options(stream_results=True),
        )
        return concepts_df

    def _get_table_concepts(self, query: select) -> pd.DataFrame:
        """
        Given a SQL query execute it and return the results in a pandas dataframe.
        Args:
            query: A SQL query to execute

        Returns:
            table_concepts_df (pd.DataFrame): A pandas dataframe containing the results of the query.
        """
        table_concepts_df = pd.read_sql_query(
            query, con=self.db_manager.engine.connect()
        )
        return table_concepts_df

    def _group_person_concepts(
        self,
        concepts: pd.DataFrame,
        person_concepts: pd.DataFrame,
        vocabulary_dict: dict,
    ) -> List[FilteringTerm]:
        """
        Merge concepts dataframe with person dataframe on "race_concept_id" and "gender_concept_id".
        Args:
            concepts (pd.DataFrame): Dataframe containing all concept_ids.
            person_concepts (pd.DataFrame): Dataframe containing all concept_ids in the Person table.
            vocabulary_dict (dict): A dictionary with the vocabulary id as key and vocabulary name as value.

        Returns:
        filters (List[FilteringTerm]) : A list of filtering terms.
        """
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
                    id_=f"{[row['vocabulary_id']]}:{row['concept_code']}",
                    label=row["concept_name"],
                    type_=vocabulary_dict[row["vocabulary_id"]],
                )
            )
        for _, row in race_df.iterrows():
            filters.append(
                FilteringTerm(
                    id_=f"{row['vocabulary_id']}:{row['concept_code']}",
                    label=row["concept_name"],
                    type_=vocabulary_dict[row["vocabulary_id"]],
                )
            )
        return filters

    def _group_filters(
        self,
        concepts: pd.DataFrame,
        omop_table_df: pd.DataFrame,
        column: str,
        vocabulary_dict: dict,
    ) -> List[FilteringTerm]:
        """
        Merge two given dataframes on the concept_id column.
        Create a list of the resulting filteringTerm objects.
        Args:
            concepts (pd.DataFrame): Dataframe containing all the concept ids.
            omop_table_df (pd.DataFrame): Dataframe containing all the concept_ids in a specific table.
            column (str): Name of the column to merge on.

        Returns:
            filters (List[FilteringTerm]) : A list of filtering terms.

        """
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
                    type_=vocabulary_dict[row["vocabulary_id"]],
                )
            )
        return filters

    def solve_concept_filters(self) -> List[FilteringTerm]:
        """
        For each OMOP table create SQL queries, build dataframe containing all concepts,group by concept_id
        and append them to a list of FilteringTerm objects
        Returns:
            final_filters (List[FilteringTerm]) : A list of filtering terms.
        """
        concepts = self._get_concepts()

        vocabulary_query = select(Vocabulary.vocabulary_id, Vocabulary.vocabulary_name)
        vocabulary = self._get_table_concepts(vocabulary_query)
        vocabulary_dict = {
            str(vocabulary_id): vocabulary_name
            for vocabulary_id, vocabulary_name in vocabulary.values
        }
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

        person_filters = self._group_person_concepts(
            concepts, person_concepts, vocabulary_dict
        )
        condition_filters = self._group_filters(
            concepts, condition, "condition_concept_id", vocabulary_dict
        )
        procedure_filters = self._group_filters(
            concepts, procedure, "procedure_concept_id", vocabulary_dict
        )
        measurement_filters = self._group_filters(
            concepts, measurement, "measurement_concept_id", vocabulary_dict
        )
        observations_filters = self._group_filters(
            concepts, observation, "observation_concept_id", vocabulary_dict
        )
        final_filters = [
            *person_filters,
            *condition_filters,
            *procedure_filters,
            *measurement_filters,
            *observations_filters,
        ]
        return final_filters


def solve_filters(db_manager: SyncDBManager) -> List[FilteringTerm]:
    """
    Extract beacon filteringTerms from OMOP db.
    Args:
        db_manager (SyncDBManager): The database manager

    Returns:
     filters (List[FilteringTerm]) : A list of filtering terms
    """
    logger = logging.getLogger(config.LOGGER_NAME)
    solver = FilterQuerySolver(db_manager=db_manager)
    try:
        filters = solver.solve_concept_filters()
        logger.info("Successfully extracted filters.")
        return filters
    except Exception as e:
        logger.error(str(e))
