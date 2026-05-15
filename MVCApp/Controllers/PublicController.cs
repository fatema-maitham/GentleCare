using Microsoft.AspNetCore.Mvc;
using MVCApp.ViewModels.Public;
using System.Net;
using System.Net.Http.Json;

namespace MVCApp.Controllers
{
    public class PublicController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PublicController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Lookup()
        {
            return View(new PublicAppointmentLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(PublicAppointmentLookupViewModel model)
        {
            model.HasSearched = true;
            model.UpcomingAppointments = new();
            model.RecentVisits = new();

            if (string.IsNullOrWhiteSpace(model.CPRNumber) ||
                string.IsNullOrWhiteSpace(model.ReferenceNumber))
            {
                model.ErrorMessage = "Please enter both CPR number and patient reference number.";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("WebAPI");

            var url =
                $"api/Appointment/lookup?cprNumber={WebUtility.UrlEncode(model.CPRNumber.Trim())}&referenceNumber={WebUtility.UrlEncode(model.ReferenceNumber.Trim())}";

            try
            {
                var appointments = await client.GetFromJsonAsync<List<PublicAppointmentResultViewModel>>(url);

                if (appointments == null || !appointments.Any())
                {
                    model.ErrorMessage = "No appointment details were found.";
                    return View(model);
                }

                model.UpcomingAppointments = appointments;
                return View(model);
            }
            catch
            {
                model.ErrorMessage = "Could not read appointment details from the API.";
                return View(model);
            }
        }
    }
}