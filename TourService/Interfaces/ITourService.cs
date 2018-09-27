using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourService.BindingModels;
using TourService.ViewModels;

namespace TourService.Interfaces
{
    public interface ITourService
    {
        Task<List<TourViewModel>> GetList();

        Task<TourViewModel> Get(int id);

        Task SendList(ReportBindingModel model);
    }
}
