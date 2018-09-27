using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourService.BindingModels;

namespace TourService.Interfaces
{
    public interface IJourneyService
    {
        Task AddElement(JourneyBindingModel jModel, ReportBindingModel rModel);

        Task SendJourneysReport(ReportBindingModel model);
    }
}
