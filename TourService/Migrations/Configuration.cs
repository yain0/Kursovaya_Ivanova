namespace TourService.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TourService.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TourService.ApplicationDbContext context)
        {
            context.Tours.Add(new TourModels.Tour
            {
                Date = DateTime.Now.AddDays(30),
                Name = "Путешествие вокруг света",
                Place = "Земля",
                Equipment = true,
                Price = 55000
            });
            context.Tours.Add(new TourModels.Tour
            {
                Date = DateTime.Now.AddDays(40),
                Name = "Путешествие в царство мертвых",
                Place = "Греция",
                Equipment = true,
                Price = 10000
            });
            context.Tours.Add(new TourModels.Tour
            {
                Date = DateTime.Now.AddDays(37),
                Name = "Путешествие к черту",
                Place = "Рашка",
                Equipment = false,
                Price = 228
            });
            context.Tours.Add(new TourModels.Tour
            {
                Date = DateTime.Now.AddDays(50),
                Name = "Тур на пересдачу к Евгению Николаевичу",
                Place = "УлГТУ",
                Equipment = true,
                Price = 1000
            });
            context.Tours.Add(new TourModels.Tour
            {
                Date = DateTime.Now.AddDays(48),
                Name = "Путешествие в новую сессию",
                Place = "Политех",
                Equipment = false,
                Price = 666
            });
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
