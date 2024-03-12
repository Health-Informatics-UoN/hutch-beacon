using Microsoft.EntityFrameworkCore;
using BeaconBridge.Models;

namespace BeaconBridge.Data;

public class BeaconContext : DbContext
{
  public BeaconContext(DbContextOptions<BeaconContext> options)
    : base(options)
  {
           
  }

  public DbSet<User> Users { get; set; }
  public DbSet<Project> Projects { get; set; }
        
        
  public DbSet<Tre> Tres { get; set; }

  public DbSet<Submission> Submissions { get; set; }
  public DbSet<HistoricStatus> HistoricStatuses { get; set; }
  public DbSet<AuditLog> AuditLogs { get; set; }
  public DbSet<SubmissionFile> SubmissionFiles { get; set; }

  public DbSet<ProjectTreDecision> ProjectTreDecisions { get; set; }

  public DbSet<MembershipTreDecision> MembershipTreDecisions { get; set; }
}
