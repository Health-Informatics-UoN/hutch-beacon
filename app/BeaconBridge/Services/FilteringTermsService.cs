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
  /// Save a list of filtering terms to the DB cache. If an entity with an ID of one of the elements
  /// already exists, it will be updated.
  /// </summary>
  /// <param name="filteringTerms">The list of filtering terms.</param>
  public async Task AddOrUpdateRangeAsync(IEnumerable<Models.FilteringTerm> filteringTerms)
  {
    foreach (var term in filteringTerms)
    {
      var existingEntity = await context.FilteringTerms.FindAsync(term.Id);
      if (existingEntity is not null)
      {
        context.Entry(existingEntity).CurrentValues.SetValues(term);
      }
      else
      {
        var newEntity = new FilteringTerm
        {
          Type = term.Type,
          Id = term.Id,
          Label = term.Label
        };
        await context.AddAsync(newEntity);
      }
    }

    await context.SaveChangesAsync();
  }
}
