using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using TourModels;
using TourService.BindingModels;
using TourService.Interfaces;

namespace TourService.Implementations
{
    public class ClientService : IClientService
    {
        private ApplicationDbContext context;

        public ClientService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public static ClientService Create(ApplicationDbContext context)
        {
            return new ClientService(context);
        }

        public async Task<IdentityResult> AddElement(ClientBindingModel model, UserManager<Client> userManager)
        {
            //Client client = await context.Users.FirstOrDefaultAsync(rec => rec.FIO.Equals(model.FIO) || rec.UserName.Equals(model.UserName));
            //if (client != null)
            //{
            //    throw new Exception("Существует пользователь с такими данными");
            //}
            Client client = new Client
            {
                FIO = model.FIO,
                UserName = model.UserName,
                PasswordHash = model.PasswordHash
            };
            return await userManager.CreateAsync(client, client.PasswordHash);
        }
    }
}
