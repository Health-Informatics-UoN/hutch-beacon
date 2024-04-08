using AutoMapper;
using BeaconBridge.Data;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Services;

public class FilteringTermsService(BeaconContext context, IMapper mapper)
{
  public async Task<List<Models.FilteringTerm>> List()
  {
    return mapper.Map<List<Models.FilteringTerm>>(
      await context.FilteringTerms
        .AsNoTracking()
        .ToListAsync());
  }
}
