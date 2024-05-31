"use server";

import { request } from "@/lib/api";

/**
 * Get the list of filtering terms from the Beacon.
 * @returns The filtering terms.
 */
export async function getFilteringTerms() {
  var searchParam = new URLSearchParams()
  searchParam.set("skip", 0)
  searchParam.set("limit", 0)
  const filteringTermsResponse = await request(
    `filtering_terms?${searchParam}`, 
    {cache: "no-store"}
  )
  return filteringTermsResponse["response"]
}

/**
 * Get the results of an individuals query using an array of filters.
 * @param {Array} filters 
 * @returns 
 */
export async function getIndividuals(filters) {
  var searchParam = new URLSearchParams()
  searchParam.set("filters", filters.map(s => s.id).join(","))

  var response = await request(`individuals?${searchParam.toString()}`)
  return response["responseSummary"]
}
