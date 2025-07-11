import { getFilteringTerms } from "@/app/actions";
import SearchForm from "@/app/components/SearchForm";

export default async function Home() {
  const filteringTerms = await getFilteringTerms()

  return(
    <div className="px-6">
      <div className="py-5">
        <p>This Beacon uses the <u><a href="https://github.com/ga4gh-beacon/beacon-v2/">GA4GH Beacon v2 API</a></u> to find individuals present in an implementation of a Trusted Research Environment (TRE), called <u><a href="https://trefx.uk/">TRE-FX.</a></u></p><br/>
        <p>TRE-FX streamlines the ability to run the same analysis across multiple TREs.  In this demonstrator, the analysis is a Beacon query (specifically, a Beacon query using only filters) that is run as a workflow over an OMOP database of synthetic COVID-19 patient EHRs.</p><br/>
        <p>There are four stages involved from sending a Beacon query to receiving a Beacon response using the TRE-FX implementation:</p>
        <p>&nbsp; 1 - The Beacon query that is received from the interface, or the API, is sent to the “submission layer”as a GA4GH TES message where it waits for ingress into the TRE.</p>
        <p>&nbsp; 2 - The TRE regularly checks the submission layer for Beacon queries. When it finds one, the query goes through security checks and is imported into the TRE.</p>
        <p>&nbsp; 3 - The TRE's TES executing agent (<a href="https://github.com/ohsu-comp-bio/funnel">Funnel</a>) uses the Beacon query in the input parameters to execute a <a href="https://github.com/Health-Informatics-UoN/hutch-beacon/tree/main/app/beacon-omop-worker">Beacon over OMOP</a> Docker image. </p>
        <p>&nbsp; 4 - To ensure no disclosive data are released from the TRE, workflow outputs are held for egress approval from the TRE owner in a manual process.  In this demonstrator, synthetic data are used so the Beacon response is automatically released.</p><br/>
        <p>The Beacon will return <span className="text-uon-forest-80">true</span> or <span className="text-uon-red-100">false</span> to indicate if the filters have been observed in any individuals.</p><br/>
        <p>Note: It may take up to 3 minutes for a Beacon response from this demonstrator.  This is due to the query waiting at the submission layer for ingress into the TRE.</p><br/>
        <p>This Beacon TRE-FX demonstrator is provided by the Centre for Health Informatics and Digital Research Service at the University of Nottingham.</p>
      </div>
      <SearchForm filteringTerms={filteringTerms}/>
    </div>
  )
}
