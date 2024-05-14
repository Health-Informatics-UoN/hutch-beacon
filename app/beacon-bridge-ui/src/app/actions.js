"use server";

import { request } from "@/lib/api";

/**
 * Get the list of filtering terms from the Beacon.
 * @returns The filtering terms.
 */
export async function getFilteringTerms() {
  var filteringTermsResponse = await request("filtering_terms")
  return filteringTermsResponse["response"]
}
