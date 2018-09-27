using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourModels;

namespace TourService
{
    public class ApplicationDbContext : IdentityDbContext<Client>
    {
        public ApplicationDbContext() : base("TourAgency")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (Exception)
            {
                foreach (var entry in ChangeTracker.Entries())
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            entry.State = EntityState.Unchanged;
                            break;
                        case EntityState.Deleted:
                            entry.Reload();
                            break;
                        case EntityState.Added:
                            entry.State = EntityState.Detached;
                            break;
                    }
                }
                throw;
            }
        }

        public virtual DbSet<Tour> Tours { get; set; }

        public virtual DbSet<Journey> Journeys { get; set; }

        public virtual DbSet<TourJourney> TourJourneys { get; set; }
    }
}
