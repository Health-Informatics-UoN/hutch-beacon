"use client";

import Header from "@/app/components/Header";
import Button from "@/app/components/Button";
import SearchDropdown from "@/app/components/SearchDropdown";
import SelectedOption from "@/app/components/SelectedOption";
import { FcSearch } from "react-icons/fc";
import { useState, useEffect } from "react";
import { getFilteringTerms } from "@/app/actions";

export default function About() {
  const icon = new FcSearch()
  
  const [filteringTerms, setFilteringTerms] = useState([])

  useEffect(
    () => {
      getFilteringTerms()
      .then(res => setFilteringTerms(res))
      .catch(error => console.error(error))
    }, []
  )
  
  const [selections, setSelections] = useState([])

  /**
   * Add a filtering term the list of selected terms.
   * @param {string} termId The id of the term to add to the selected list.
   */
  function addSelection(termId) {
    let term = filteringTerms.find(ft => ft.id === termId)
    setSelections(previous => [term, ...previous])
  }

  /**
   * Remove a filtering term from the list of selected terms.
   * @param {object} term The term to remove from the list of selected terms.
   */
  function removeSelection(term) {
    let selectionsCopy = [...selections]
    let termIndex = selectionsCopy.indexOf(term)
    if (termIndex > -1) {
      selectionsCopy.splice(termIndex, 1)
      setSelections(selectionsCopy)
    }
  }

  return(
    <div>
      <Header />
      <div>
        <h1>About</h1>
        <p>This Beacon uses the GA4GH Beacon v2 API specification and is provided by the Centre for Health Informatics and Digital Research Service at the University of Nottingham.</p>
        <p>Beacon filters can be used to query a backend OMOP database of synthetic COVID-19 patient EHRs (electronic health records).</p>
        <p>The Beacon will return <span className="text-green-400">yes</span> or <span className="text-red-400">no</span> to indicate if the filters have been observed in any individuals.</p>
      </div>
      <div>
        <h1>Query</h1>
        <SearchDropdown
          id={"filtering-terms"}
          label={"Filtering terms"}
          options={filteringTerms}
          onChange={addSelection} />
        <Button icon={icon} text={"Search"}/>
      </div>
      {selections.length > 0 &&
        <div>
          <h2>Selected</h2>
          {
            selections.map((s, key) => {
              return <SelectedOption key={key} option={s} removeFunction={removeSelection}/>
            })
          }
        </div>
      }
    </div>
  )
}
