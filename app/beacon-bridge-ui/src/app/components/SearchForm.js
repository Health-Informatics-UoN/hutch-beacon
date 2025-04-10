"use client";

import { useState } from "react";
import InfoPopup from "@/app/components/InfoPopup";
import Button from "@/app/components/Button";
import SearchDropdown from "@/app/components/SearchDropdown";
import SelectedOption from "@/app/components/SelectedOption";
import { FaSearch } from "react-icons/fa";
import { FaRegTrashAlt } from "react-icons/fa";
import { getIndividuals } from "@/app/actions";

export default function SearchForm({ filteringTerms }) {
  const [hasResults, setHasResults] = useState();
  const [selections, setSelections] = useState([]);
  const [isLoading, setLoading] = useState(false);

  /**
   * Handle fetching individuals on search button click.
   */
  async function fetchIndividuals() {
    try {
      setLoading(true);
      setHasResults(undefined);
      var res = await getIndividuals(selections);
      setHasResults(res["exists"]);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  }

  /**
   * Add a filtering term the list of selected terms.
   * @param {string} termId The id of the term to add to the selected list.
   */
  function addSelection(termId) {
    setHasResults(undefined); // modified the query to reset the results box
    let term = filteringTerms.find((ft) => ft.id === termId);
    setSelections((previous) => [term, ...previous]);
  }

  /**
   * Remove a filtering term from the list of selected terms.
   * @param {object} term The term to remove from the list of selected terms.
   */
  function removeSelection(term) {
    setHasResults(undefined); // modified the query to reset the results box
    let selectionsCopy = [...selections];
    let termIndex = selectionsCopy.indexOf(term);
    if (termIndex > -1) {
      selectionsCopy.splice(termIndex, 1);
      setSelections(selectionsCopy);
    }
  }

  /**
   * Clear current search selections and cause the results box to vanish.
   */
  function clearSelected() {
    setSelections([]);
    setHasResults(undefined); // modified the query to reset the results box
  }

  return (
    <>
      <div>
        <h2 className="mb-4 text-xl md:text-2xl">Query</h2>
        <h3 className="mb-4 text-xl md:text-2xl">Select Filtering Terms:</h3>
        <SearchDropdown
          id={"filtering-terms"}
          options={filteringTerms}
          onChange={addSelection}
        />
        {hasResults !== undefined && (
          <InfoPopup
            isWarning={!hasResults}
            text={
              hasResults
                ? "There are individuals matching your query."
                : "There are no individuals matching your query."
            }
            className="rounded-lg border-2 border-uon-blue-60 border-solid mb-4"
          />
        )}
        <span className="flex space-x-2">
          <Button
            icon={
              isLoading ? (
                <loader className={inter.className} />
              ) : (
                <FaSearch size={18} />
              )
            }
            text={isLoading ? "Loading.." : "Search"}
            onClick={fetchIndividuals}
            disabled={isLoading}
            className="
            flex items-center justify-center
            w-32 px-4 py-2
            bg-uon-sky-100 hover:bg-uon-sky-80
            text-white font-medium
            rounded-lg shadow-md transition
            disabled:opacity-50 disabled:cursor-not-allowed"
          />

          <Button
            icon={<FaRegTrashAlt />}
            text={"Reset"}
            onClick={clearSelected}
            className=" flex items-center justify-center
            w-32 px-4 py-2
            bg-uon-red-100 hover:bg-uon-red-80
            text-white font-medium
            rounded-lg shadow-md transition
            disabled:opacity-50 disabled:cursor-not-allowed"
          />
        </span>
      </div>
      {selections.length > 0 && (
        <div>
          <h2 className="mb-4 text-lg md:text-xl py-4">Selected Filters:</h2>
          {selections.map((s, key) => {
            return (
              <SelectedOption
                key={key}
                option={s}
                removeFunction={removeSelection}
              />
            );
          })}
        </div>
      )}
    </>
  );
}
