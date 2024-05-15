import Header from "@/app/components/Header";
import { getFilteringTerms, getIndividuals } from "@/app/actions";
import SearchForm from "../components/SearchForm";

export default async function About() {
  const filteringTerms = await getFilteringTerms()

  return(
    <div>
      <Header />
      <div>
        <h1>About</h1>
        <p>This Beacon uses the GA4GH Beacon v2 API specification and is provided by the Centre for Health Informatics and Digital Research Service at the University of Nottingham.</p>
        <p>Beacon filters can be used to query a backend OMOP database of synthetic COVID-19 patient EHRs (electronic health records).</p>
        <p>The Beacon will return <span className="text-green-400">yes</span> or <span className="text-red-400">no</span> to indicate if the filters have been observed in any individuals.</p>
      </div>
      <SearchForm filteringTerms={filteringTerms}/>
    </div>
  )
}
