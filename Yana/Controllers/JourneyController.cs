using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TourService;
using TourService.BindingModels;
using TourService.Implementations;
using TourService.Interfaces;

namespace Yana.Controllers
{
    [Authorize]
    public class JourneyController : ApiController
    {
        #region global
        private string _tempPath;

        public string TempPath
        {
            get => string.IsNullOrEmpty(_tempPath) ? getTemp() : _tempPath;
        }

        private string _resourcesPath;

        public string ResourcesPath
        {
            get => string.IsNullOrEmpty(_resourcesPath) ? getResourses() : _resourcesPath;
        }

        private string getTemp()
        {
            _tempPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Temp/");
            return _tempPath;
        }

        private string getResourses()
        {
            _resourcesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/");
            return _resourcesPath;
        }

        private ApplicationDbContext _context;

        public ApplicationDbContext Context
        {
            get
            {
                return _context ?? Request.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _context = value;
            }
        }

        private IJourneyService _service;

        public IJourneyService Service
        {
            get => _service ?? JourneyService.Create(Context);
            private set
            {
                _service = value;
            }
        }
        #endregion

        [HttpPost]
        public async Task AddElement(List<TourJourneyBindingModel> tourJourneys)
        {
            await Service.AddElement(new JourneyBindingModel
            {
                ClientId = User.Identity.GetUserId(),
                TourJourneys = tourJourneys
            }, new ReportBindingModel {
                FilePath = TempPath,
                FontPath = ResourcesPath + "TIMCYR.TTF"
            });
        }

        [HttpPost]
        public async Task SendJourneysReport(ReportBindingModel model)
        {
            model.FilePath = TempPath;
            model.FontPath = ResourcesPath + "TIMCYR.TTF";
            model.ClientId = User.Identity.GetUserId();
            await Service.SendJourneysReport(model);
        }
    }
}
