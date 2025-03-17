from typing import List
from beacon_omop_worker.beacon_dto.filtering_term import FilteringTerm
from beacon_omop_worker.beacon_dto.response_summary import ResponseSummary
from beacon_omop_worker.db_manager import SyncDBManager
from beacon_omop_worker.entities import (
    ConditionOccurrence,
    Person,
    Concept,
    Vocabulary,
    Location,
    Death
)
import beacon_omop_worker.settings as settings
import logging
import pandas as pd
from lifelines import KaplanMeierFitter
import matplotlib.pyplot as plt
from sqlalchemy import select, and_


class IndividualQuerySolver:

    def __init__(self, db_manager: SyncDBManager) -> None:
        self.db_manager = db_manager

    def _get_concept_id(self, vocab: str, concept_code: str) -> str:
        """
        Get concept id from vocabulary and concept code
        Args:
            vocab (str): vocabulary name
            concept_code (str): concept code

        Returns:

        """
        query = select(Concept.concept_id).where(
            and_(
                Concept.vocabulary_id == vocab,
                Concept.concept_code == concept_code,
            )
        )
        code = pd.read_sql(query, con=self.db_manager.engine.connect())
        final_code = str(code["concept_id"].values[0])
        return final_code

    def solve_individual_query(self, query_terms: str) -> ResponseSummary:
        """
        Build sql query based on query terms, run query and return response summary.
        Args:
            query_terms (str): Query filter terms

        Returns:
        response_summary (ResponseSummary): Response summary object
        """
        terms = query_terms.split(",")
        concept_codes = list()
        # build main query
        main_query = select(Person)
        for term in terms:

            filtering_term = term.split(":")
            if filtering_term[0] == "SNOMED":
                concept_codes.append(filtering_term[1])
            if filtering_term[0] == "Gender":
                gender_concept_id = self._get_concept_id(
                    vocab=filtering_term[0], concept_code=filtering_term[1]
                )

                gender_query = select(Person.person_id).where(
                    Person.gender_concept_id == gender_concept_id
                )
                gender_ids = pd.read_sql(
                    gender_query, con=self.db_manager.engine.connect()
                )
                gender_id = [str(concept) for concept, in gender_ids.values]
                # add to main query
                main_query = main_query.where(Person.person_id.in_(gender_id))

            if filtering_term[0] == "Race":
                race_concept_id = self._get_concept_id(
                    vocab=filtering_term[0], concept_code=filtering_term[1]
                )
                race_query = select(Person.person_id).where(
                    Person.race_concept_id == race_concept_id
                )
                race_ids = pd.read_sql(race_query, con=self.db_manager.engine.connect())
                race_id = [str(concept) for concept, in race_ids.values]
                # add to main query
                main_query = main_query.where(Person.person_id.in_(race_id))

        # get concept ids from concept codes
        sql_query = select(Concept.concept_id).where(
            and_(
                Concept.vocabulary_id == "SNOMED",
                Concept.concept_code.in_(concept_codes),
            )
        )

        concept_ids = pd.read_sql_query(sql_query, con=self.db_manager.engine.connect())
        concept_id_list = [str(concept) for concept, in concept_ids.values]

        for concept in concept_id_list:
            results_query = select(ConditionOccurrence.person_id).where(
                ConditionOccurrence.condition_concept_id == concept
            )
            # add to main query
            main_query = main_query.where(Person.person_id.in_(results_query))

        # execute main query
        person_ids = pd.read_sql_query(main_query, con=self.db_manager.engine.connect())
        if person_ids.empty:
            # if no matching records found return false
            return ResponseSummary(exists=False)
        return ResponseSummary(exists=True)


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
            if row["gender_concept_id"] != 0:
                filters.append(
                    FilteringTerm(
                        id_=f"{row['vocabulary_id']}:{row['concept_code']}",
                        label=row["concept_name"],
                        type_=vocabulary_dict[row["vocabulary_id"]],
                    )
                )
        for _, row in race_df.iterrows():
            if row["race_concept_id"] != 0:
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
            if row[column] != 0:
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

        person_filters = self._group_person_concepts(
            concepts, person_concepts, vocabulary_dict
        )
        condition_filters = self._group_filters(
            concepts, condition, "condition_concept_id", vocabulary_dict
        )
        final_filters = [*person_filters, *condition_filters]
        return final_filters


def solve_filters(db_manager: SyncDBManager) -> List[FilteringTerm]:
    """
    Extract beacon filteringTerms from OMOP db.
    Args:
        db_manager (SyncDBManager): The database manager

    Returns:
     filters (List[FilteringTerm]) : A list of filtering terms
    """
    logger = logging.getLogger(settings.LOGGER_NAME)
    solver = FilterQuerySolver(db_manager=db_manager)
    try:
        filters = solver.solve_concept_filters()
        logger.info("Successfully extracted filters.")
        return filters
    except Exception as e:
        logger.error(str(e))


def solve_individuals(db_manager: SyncDBManager, query_terms: str):
    """
    Solve individual query
    Args:
        db_manager (SyncDBManager): The database manager
        query_terms (str): The query terms

    Returns:

    """
    logger = logging.getLogger(settings.LOGGER_NAME)
    query_solver = IndividualQuerySolver(db_manager=db_manager)
    try:
        response_summary = query_solver.solve_individual_query(query_terms)
        logger.info("Successfully executed query.")
        return response_summary
    except Exception as e:
        logger.error(str(e))

class KaplanMeierQuerySolver:

    def __init__(self, db_manager) -> None:
        self.db_manager = db_manager

    def _get_concept_id(self, vocab: str, concept_code: str) -> str:
        """
        Get concept_id from vocabulary and concept code.
        
        Args:
            vocab (str): The vocabulary name (e.g., 'SNOMED').
            concept_code (str): The concept code (e.g., '386661006').
        
        Returns:
            str: The concept_id corresponding to the vocabulary and concept code.
        """
        logger = logging.getLogger(settings.LOGGER_NAME)
        try:
            query = select(Concept.concept_id).where(
                and_(
                    Concept.vocabulary_id == vocab,
                    Concept.concept_code == concept_code
                )
            )
            result = pd.read_sql(query, con=self.db_manager.engine.connect())
            concept_id = str(result["concept_id"].values[0])
            logger.info(f"Found concept_id {concept_id} for {vocab}:{concept_code}.")
            return concept_id
        except Exception as e:
            logger.error(f"Error fetching concept_id for {vocab}:{concept_code}: {e}")
            raise

    def _get_strata(self, strata: str) -> str:
        """
        Get the strata column name from the strata input.
        
        Args:
            strata (str): The strata input.
        
        Returns:
            str: The strata column name or None if not applicable.
        """
        logger = logging.getLogger(settings.LOGGER_NAME)
        try:
            if strata is None:
                return None
            if strata.lower() == 'gender':
                return 'gender_source_value'
            elif strata.lower() == 'location':
                return 'city'
            else:
                return None  # Return None if it's not gender or location
        except Exception as e:
            logger.error(f"Error determining strata column: {e}")
            raise e

    def fetch_condition_data(self, concept_id: str) -> pd.DataFrame:
        """
        Fetch data from the OMOP database based on concept_id.
        
        Args:
            concept_id (str): The concept_id for the condition.
        
        Returns:
            pd.DataFrame: DataFrame containing the condition data.
        """
        logger = logging.getLogger(settings.LOGGER_NAME)
        
        try:
            # Create the queries 
            condition_occurrence_query = select(ConditionOccurrence).where(
                ConditionOccurrence.condition_concept_id == concept_id
            )
            
            concept_query = select(Concept.concept_id, Concept.concept_name)
            
            person_query = select(
                Person.person_id, Person.birth_datetime, Person.gender_source_value, 
                Person.race_source_value, Person.location_id
            )
            
            location_query = select(Location.location_id, Location.city, Location.county)
            
            death_query = select(Death.person_id, Death.death_date, Death.cause_concept_id)
            
            # Execute the queries and load the results into pandas DataFrames
            condition_occurrence_df = pd.read_sql(
                condition_occurrence_query,
                con=self.db_manager.engine.connect()
            )
            
            concept_df = pd.read_sql(
                concept_query,
                con=self.db_manager.engine.connect()
            )
            
            person_df = pd.read_sql(
                person_query,
                con=self.db_manager.engine.connect()
            )
            
            location_df = pd.read_sql(
                location_query,
                con=self.db_manager.engine.connect()
            )
            
            death_df = pd.read_sql(
                death_query,
                con=self.db_manager.engine.connect()
            )
            
            # Set indexes for efficient merging
            condition_occurrence_df.set_index('condition_concept_id', inplace=True)
            concept_df.set_index('concept_id', inplace=True)
            person_df.set_index('person_id', inplace=True)
            location_df.set_index('location_id', inplace=True)
            death_df.set_index('person_id', inplace=True)
            
            # Perform joins using pandas
            merged_df = condition_occurrence_df.join(
                concept_df, on="condition_concept_id", how="inner"
            ).join(
                person_df, on="person_id", how="inner"
            ).join(
                location_df, on="location_id", how="inner"
            ).join(
                death_df, on="person_id", how="left"
            )
            
            # Reset index to get a clean DataFrame
            merged_df.reset_index(inplace=True)
            
            # Select and rename the necessary columns
            result_df = merged_df[[
                "condition_occurrence_id",
                "condition_concept_id",
                "concept_name",
                "condition_start_date",
                "condition_end_date",
                "birth_datetime",
                "gender_source_value",
                "race_source_value",
                "city",
                "county",
                "death_date",
                "cause_concept_id"
            ]]
            
            logger.info(f"Successfully fetched condition data for concept_id {concept_id}.")
            return result_df
        
        except Exception as e:
            logger.error(f"Error fetching condition data: {e}")
            raise e

    def calculate_duration_and_observed(self, data: pd.DataFrame) -> pd.DataFrame:
        """
        Calculate the duration and observed (event) for the Kaplan-Meier analysis.
        
        Args:
            data (pd.DataFrame): DataFrame containing condition start and end dates.
        
        Returns:
            pd.DataFrame: Updated DataFrame with 'duration' and 'observed' columns.
        """
        logger = logging.getLogger(settings.LOGGER_NAME)
        try:
            # Drop entries without start_date
            data = data.dropna(subset=['condition_start_date'])

            # Ensure dates are in correct format
            data['condition_start_date'] = pd.to_datetime(data['condition_start_date'])
            data['condition_end_date'] = pd.to_datetime(data['condition_end_date'])

            # Handle censored data
            data['observed'] = data['condition_end_date'].isna().apply(lambda x: 0 if x else 1)
            censored_date = pd.Timestamp('2023-12-31')  # Set a fixed censor date
            data['condition_end_date'] = data['condition_end_date'].fillna(censored_date)

            # Calculate duration
            data['duration'] = ((data['condition_end_date'] - data['condition_start_date']).dt.days)
            return data
        
        except Exception as e:
            logger.error(f"Error calculating duration and observed: {e}")
            raise e

    def plot_km(self, data: pd.DataFrame, concept_id: str, strata: str, file_path: str) -> None:
        """
        Plot the Kaplan-Meier curve for a specific medical condition and save it to a file.
        
        Args:
            data (pd.DataFrame): The data containing condition information with 'duration' and 'observed' columns.
            concept_id (str): The concept_id for the condition.
            file_path (str): The file path where the plot will be saved.
        """
        logger = logging.getLogger(settings.LOGGER_NAME)
        try:
            # Kaplan-Meier estimator fitting
            event_times = data['duration']
            event_observed = data['observed']

            # Initialize the figure and axis
            fig, ax = plt.subplots(figsize=(8, 6))

            if strata is not None:
                kmf_list = []
                strata_values = data[strata].value_counts().nlargest(3).index
                strata_labels = [str(value) for value in strata_values]
                for i, stratum_value in enumerate(strata_values):
                    kmf = KaplanMeierFitter()
                    T = data['duration'][data[strata] == stratum_value]
                    E = data['observed'][data[strata] == stratum_value]
                    kmf.fit(T, E, label=stratum_value)
                    ax = kmf.plot(ax=ax, ci_show=True)
                    kmf_list.append(kmf)

                    # Add the median survival time line
                    median_survival_time = kmf.median_survival_time_
                    plt.axvline(median_survival_time, color=f'C{i}', linestyle='--', label=f'Median {stratum_value}: {median_survival_time:.2f}')
            else:
                kmf = KaplanMeierFitter()
                kmf.fit(event_times, event_observed, label='All patients')
                ax = kmf.plot(ax=ax, ci_show=True)

                # Add the median survival time line
                median_survival_time = kmf.median_survival_time_
                plt.axvline(median_survival_time, color='r', linestyle='--', label=f'Median: {median_survival_time:.2f}')

            plt.xlabel('Time (days)')
            plt.ylabel('Survival Probability')
            plt.title(f'Kaplan-Meier Curve for {data["concept_name"].iloc[0]}')

            plt.xlim(0, 15)
            plt.xticks(ticks=range(0, 15, 1))
            plt.yticks(ticks=[0, 0.2, 0.4, 0.6, 0.8, 1])
            plt.legend(loc = 'best')

            # Save the plot to the specified file
            plt.savefig(file_path, format='png')
            plt.close()

            logger.info(f"Kaplan-Meier plot saved to {file_path} for concept_id {concept_id}.")
        except Exception as e:
            logger.error(f"Error while plotting and saving Kaplan-Meier curve: {e}")
            raise e

# Main function to generate Kaplan-Meier analysis
def generate_km(db_manager, vocabulary: str, strata: str, output_name: str):
    """
    Fetch data and generate a Kaplan-Meier plot for a given SNOMED code, saving it to a file.
    
    Args:
        db_manager (SyncDBManager): The database manager.
        vocabulary (str): The SNOMED code input, e.g., "SNOMED:386661006".
        strata (str): The strata input for Kaplan-Meier analysis (e.g., 'gender', 'location').
        output_name (str): The file path where the Kaplan-Meier plot will be saved.
    """
    logger = logging.getLogger(settings.LOGGER_NAME)
    solver = KaplanMeierQuerySolver(db_manager=db_manager)

    try:
        # Extract the concept_id from the SNOMED code
        vocab, code = vocabulary.split(":")
        concept_id = solver._get_concept_id(vocab, code)
        
        # Get the strata column name
        strata_column = solver._get_strata(strata)

        # Fetch condition data for the concept_id
        condition_data = solver.fetch_condition_data(concept_id)

        # Calculate duration and observed values for the data
        condition_data = solver.calculate_duration_and_observed(condition_data)

        # Generate and save the Kaplan-Meier plot
        solver.plot_km(condition_data, concept_id, strata_column, output_name)
        
        logger.info(f"Kaplan-Meier analysis successfully generated for {vocabulary}")
    except Exception as e:
        logger.error(f"Error generating Kaplan-Meier plot for {vocabulary}: {e}")
        raise e
