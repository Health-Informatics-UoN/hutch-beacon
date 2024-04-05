using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Data;

public class BeaconContext : DbContext
{
  public BeaconContext(DbContextOptions<SubmissionContext> options) : base(options)
  {
  }

  public BeaconContext()
  {
  }
}
