
using Microsoft.AspNetCore.Mvc;
using GLTest.Client.Services;
using GLTest.Client.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GLTest.Client.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
 
        private IActionResult HandleResult<T>(Result<T> result, string viewName, object? model = null)
        {
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                ModelState.AddModelError(string.Empty, result.Message);
                return View(viewName, model ?? result.Data);
            }
            return View(viewName, result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var result = await _companyService.GetCompaniesAsync();
            return HandleResult(result, "List");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _companyService.GetCompanyAsync(id);
            return HandleResult(result, "Edit");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Company model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _companyService.UpdateCompanyAsync(model);
            return result.Success ? RedirectToAction("List") : HandleResult(result, "Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Company model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Id = Guid.NewGuid();
            model.UpdatedBy = User.Identity?.Name ?? "SYSTEM";

            var result = await _companyService.CreateCompanyAsync(model);
            return result.Success ? RedirectToAction("List") : HandleResult(result, "Create", model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(Guid? id, string? isin)
        {
            if (!id.HasValue && string.IsNullOrWhiteSpace(isin))
                return View();

            var result = id.HasValue
                ? await _companyService.GetCompanyAsync(id.Value)
                : await _companyService.GetCompanyByIsinAsync(isin!);

            return result.Success && result.Data != null ? View(result.Data) : HandleResult(result, "Search");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}





//using Microsoft.AspNetCore.Mvc;
//using GLTest.Client.Services;
//using GLTest.Client.Models;
//using Microsoft.AspNetCore.Authorization;

//namespace GLTest.Client.Controllers
//{
//    [Authorize]
//    public class CompanyController : Controller
//    {

//        private readonly ICompanyService _companyService;

//        public CompanyController(ICompanyService companyService)
//        {
//            _companyService = companyService;
//        }
//        [HttpGet]
//        public async Task<IActionResult> List()
//        {
//            var result = await _companyService.GetCompaniesAsync();
//            if (!result.Success) ViewBag.Error = result.Message;

//            return View(result.Data);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(Guid id)
//        {
//            var result = await _companyService.GetCompanyAsync(id);
//            if (!result.Success) ViewBag.Error = result.Message;
//            return View(result.Data);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(Company model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);


//            var result = await _companyService.UpdateCompanyAsync(model);

//            if (!result.Success)
//            {
//                ViewBag.Error = result.Message;
//                ModelState.AddModelError(string.Empty, result.Message);
//                return View(model);
//            }

//            return RedirectToAction("List");
//        }


//        [HttpPost]
//        public async Task<IActionResult> Create(Company model)
//        {
//             if (!ModelState.IsValid)
//                return View(model);


//            model.Id = Guid.NewGuid();
//            model.UpdatedBy = User.Identity?.Name ?? "SYSTEM";
//            var result = await _companyService.CreateCompanyAsync(model);

//            if (!result.Success)
//            {
//                ViewBag.Error = result.Message;
//                ModelState.AddModelError(string.Empty, result.Message);
//                return View(model);
//            }

//            return RedirectToAction("List");
//        }

//        [HttpGet]
//        public async Task<IActionResult> Search(Guid? id, string? isin)
//        {
//            if (!id.HasValue && string.IsNullOrWhiteSpace(isin))
//            {
//                return View();
//            }

//            Result<Company> result;

//            if (id.HasValue)
//            {
//                result = await _companyService.GetCompanyAsync(id.Value);
//            }
//            else if (!string.IsNullOrWhiteSpace(isin))
//            {
//                result = await _companyService.GetCompanyByIsinAsync(isin);
//            }
//            else
//            {
//                return View();
//            }

//            if (!result.Success || result.Data == null)
//            {
//                ViewBag.Error = result.Message; 
//                return View();
//            }

//            return View(result.Data);
//        }


//        [HttpGet]
//        public IActionResult Create()
//        {
//            return View();
//        }


//    }
//}



