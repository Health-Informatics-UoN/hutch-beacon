import { getFilteringTerms } from "@/app/actions";
import SearchForm from "@/app/components/SearchForm";

export default async function Home() {
  const filteringTerms = await getFilteringTerms()

  return(
    <div className="px-6">
      <div>
        <h1 className="mb-4 text-xl md:text-2xl">About</h1>
        <p>This Beacon uses the GA4GH Beacon v2 API specification and is provided by the Centre for Health Informatics and Digital Research Service at the University of Nottingham.</p>
        <p>Beacon filters can be used to query a backend OMOP database of synthetic COVID-19 patient EHRs (electronic health records).</p>
        <p>The Beacon will return <span className="text-uon-forest-80">true</span> or <span className="text-uon-red-100">false</span> to indicate if the filters have been observed in any individuals.</p>
      </div>
      <SearchForm filteringTerms={filteringTerms}/>
    </div>
  )
}
