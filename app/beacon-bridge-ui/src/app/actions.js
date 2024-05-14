"use server";

import { request } from "@/lib/api";

export async function getFilteringTerms() {
  var filteringTermsResponse = await request("filtering_terms")
  return filteringTermsResponse["response"]
}
