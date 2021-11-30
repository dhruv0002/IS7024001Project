using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using QuickType;
using QuickTypeNobelLaureates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public bool SearchCompleted { get; set; }
        public string CountryQuery { get; set; } 
        public NobelLaureates NobelLaureates { get; set; }

        public static SelectList SelectListItems; 

        public static Dictionary<string, string> CountryDictionary; 
        public IActionResult OnGet(string countryQuery)
        {
            
            if(SelectListItems == null || !SelectListItems.Any())
            {
               
                using (var webClient = new WebClient())
                {
                    string jsonString = "";
                    try
                    {
                        jsonString = webClient.DownloadString("https://pkgstore.datahub.io/core/country-list/data_json/data/8c458f2d15d9f2119654b29ede6e45b8/data_json.json");
                        SelectListItems = new SelectList(Country.FromJson(jsonString), "Code", "Name");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                        SelectListItems = new SelectList(new List<Country>(), "Code", "Name");
                    }

                }
            } 

            ViewData["Code"] = SelectListItems;

            if (!string.IsNullOrWhiteSpace(countryQuery))
            {
                if (CountryDictionary == null || !CountryDictionary.Any())
                {
                    CountryDictionary = SelectListItems.ToDictionary(x => x.Value, x => x.Text);
                }

                CountryQuery = CountryDictionary[countryQuery];
            }

            SearchCompleted = false;

            if (!string.IsNullOrWhiteSpace(countryQuery))
            {
                using (var webClient = new WebClient())
                {
                    string jsonString = "";
                    try
                    {
                        jsonString = webClient.DownloadString($"https://api.nobelprize.org/v1/laureate.json?bornCountryCode={countryQuery}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                    }

                    NobelLaureates = NobelLaureates.FromJson(jsonString);

                }

                if (NobelLaureates != null && NobelLaureates.Laureates.Any())
                {
                    SearchCompleted = true;
                }
               
            }
        
            return Page();
        }
    }
}
