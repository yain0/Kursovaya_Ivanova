using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using TourModels;
using TourService;
using Yana.Models;

namespace Yana
{
    // Настройка диспетчера пользователей приложения. UserManager определяется в ASP.NET Identity и используется приложением.

    public class ApplicationUserManager : UserManager<Client>
    {
        public ApplicationUserManager(IUserStore<Client> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<Client>(context.Get<ApplicationDbContext>()));
            // Настройка логики проверки имен пользователей
            manager.UserValidator = new UserValidator<Client>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            // Настройка логики проверки паролей
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<Client>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }
}
