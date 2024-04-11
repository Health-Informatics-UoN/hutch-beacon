using AutoMapper;
using BeaconBridge.Data;
using BeaconBridge.Data.Entities;
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

  /// <summary>
  /// Save a list of filtering terms to the DB cache.
  /// </summary>
  /// <param name="filteringTerms">The list of filtering terms.</param>
  public async Task SaveRangeAsync(IEnumerable<Models.FilteringTerm> filteringTerms)
  {
    var termEntities = filteringTerms.Select(term => new FilteringTerm
    {
      Type = term.Type,
      Id = term.Id,
      Label = term.Label,
      Scope = term.Scope
    });
    foreach (var entity in termEntities)
    {
      await context.AddAsync(entity);
    }

    await context.SaveChangesAsync();
  }
}
