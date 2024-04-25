import os
import sys
import logging
import beacon_omop_worker.config as config
import json
import argparse
from beacon_omop_worker.db_manager import SyncDBManager
from beacon_omop_worker import query_solvers

parser = argparse.ArgumentParser(
    prog="beacon-omop-worker",
    description="This program executes beacon queries against an OMOP database",
)
subparsers = parser.add_subparsers(
    dest="command", help="Commands to run", required=True
)
filters = subparsers.add_parser("filteringterms", help="Extract filtering terms")
filters.set_defaults()


def save_to_output(filtering_terms: list, destination: str) -> None:
    """Save the result to a JSON file.

    Args:
        filtering_terms (list): The object containing the result of a query.
        destination (str): The name of the JSON file to save the results.

    Raises:
        ValueError: A path to a non-JSON file was passed as the destination.
    """
    if not destination.endswith(".json"):
        raise ValueError("Please specify a JSON file (ending in '.json').")
    logger = logging.getLogger(config.LOGGER_NAME)
    try:
        with open(destination, "w") as output_file:
            file_body = json.dumps(
                [filteringTerm.__dict__ for filteringTerm in filtering_terms]
            )
            output_file.write(file_body)
    except Exception as e:
        logger.error(str(e), exc_info=True)


def main() -> None:
    # Set up the logger
    log_format = logging.Formatter(
        config.MSG_FORMAT,
        datefmt=config.DATE_FORMAT,
    )
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(log_format)
    logger = logging.getLogger(config.LOGGER_NAME)
    logger.setLevel(logging.INFO)
    logger.addHandler(console_handler)

    # Parse command line arguments
    args = parser.parse_args()

    logger.info("Setting up database connection...")
    datasource_db_port = os.getenv("DATASOURCE_DB_PORT")
    db_manager = SyncDBManager(
        username=os.getenv("DATASOURCE_DB_USERNAME"),
        password=os.getenv("DATASOURCE_DB_PASSWORD"),
        host=os.getenv("DATASOURCE_DB_HOST"),
        port=int(datasource_db_port) if datasource_db_port is not None else None,
        database=os.getenv("DATASOURCE_DB_DATABASE"),
        drivername=os.getenv("DATASOURCE_DB_DRIVERNAME", config.DEFAULT_DB_DRIVER),
        schema=os.getenv("DATASOURCE_DB_SCHEMA"),
    )
    #
    if args.command == "filteringterms":
        output_file_name = "output.json"
        logger.info("Extracting filtering terms...")
        filtering_terms = query_solvers.solve_filters(db_manager=db_manager)
        try:
            logger.info("Saving filtering terms to file...")
            save_to_output(filtering_terms, output_file_name)
            logger.info(f"Saved results to {output_file_name}")
        except ValueError as e:
            logger.error(str(e), exc_info=True)
