using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using TourService;
using TourService.Interfaces;
using TourService.Implementations;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Yana.Controllers
{
    [Authorize]
    public class TourController : ApiController
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

        private ITourService _service;

        public ITourService Service
        {
            get => _service ?? TourService.Implementations.TourService.Create(Context);
            private set
            {
                _service = value;
            }
        }
        #endregion

        [HttpGet]
        public async Task<IHttpActionResult> GetList()
        {
            var list = await Service.GetList();
            if (list == null)
            {
                InternalServerError(new Exception("Нет данных"));
            }
            return Ok(list);
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            var element = await Service.Get(id);
            if (element == null)
            {
                InternalServerError(new Exception("Нет данных"));
            }
            return Ok(element);
        }

        [HttpPost]
        public async Task<IHttpActionResult> SendList()
        {
            await Service.SendList(new TourService.BindingModels.ReportBindingModel
            {
                ClientId = User.Identity.GetUserId(),
                FilePath = TempPath,
                FontPath = ResourcesPath + "TIMCYR.TTF"
            });
            return Ok();
        }
    }
}
