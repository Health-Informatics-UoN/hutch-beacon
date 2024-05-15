"use client";

import { useState } from "react";
import InfoPopup from "@/app/components/InfoPopup";
import Button from "@/app/components/Button";
import SearchDropdown from "@/app/components/SearchDropdown";
import SelectedOption from "@/app/components/SelectedOption";
import { FcSearch } from "react-icons/fc";
import { getIndividuals } from "@/app/actions";

export default function SearchForm({ filteringTerms }) {
  const [hasResults, setHasResults] = useState();
  const [selections, setSelections] = useState([]);

  /**
   * Handle fetching individuals on search button click.
   */
  async function fetchIndividuals() {
    try {
      var res = await getIndividuals(selections);
      setHasResults(res["exists"]);
    } catch (error) {
      console.error(error);
    } finally {
      // Clear selections
      setSelections([]);
    }
  }

  /**
   * Add a filtering term the list of selected terms.
   * @param {string} termId The id of the term to add to the selected list.
   */
  function addSelection(termId) {
    let term = filteringTerms.find((ft) => ft.id === termId);
    setSelections((previous) => [term, ...previous]);
  }

  /**
   * Remove a filtering term from the list of selected terms.
   * @param {object} term The term to remove from the list of selected terms.
   */
  function removeSelection(term) {
    let selectionsCopy = [...selections];
    let termIndex = selectionsCopy.indexOf(term);
    if (termIndex > -1) {
      selectionsCopy.splice(termIndex, 1);
      setSelections(selectionsCopy);
    }
  }

  return (
    <>
      <div>
        <h1 className="mb-4 text-xl md:text-2xl">Query</h1>
        <SearchDropdown
          id={"filtering-terms"}
          label={"Select filtering terms"}
          options={filteringTerms}
          onChange={addSelection}
        />
        <Button
          icon={<FcSearch />}
          text={"Search"}
          onClick={fetchIndividuals}
          className="w-24 bg-uon-sky-100 px-2 py-2 rounded-lg"
        />
        {hasResults !== undefined && (
          <InfoPopup
            isWarning={!hasResults}
            text={
              hasResults
                ? "There are individuals matching your query."
                : "There are no individuals matching your query."
            }
          />
        )}
      </div>
      {selections.length > 0 && (
        <div>
          <h2 className="mb-4 text-lg md:text-xl">Selected</h2>
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
