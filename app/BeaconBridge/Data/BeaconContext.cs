using BeaconBridge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Data;

public class BeaconContext : DbContext
{
  public BeaconContext(DbContextOptions<BeaconContext> options) : base(options)
  {
  }

  public BeaconContext()
  {
  }

  public DbSet<FilteringTerm> FilteringTerms => Set<FilteringTerm>();
}
