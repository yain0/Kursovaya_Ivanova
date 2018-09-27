using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourModels;
using TourService.BindingModels;

namespace TourService.Interfaces
{
    public interface IClientService
    {
        Task<IdentityResult> AddElement(ClientBindingModel model, UserManager<Client> userManager);
    }
}
