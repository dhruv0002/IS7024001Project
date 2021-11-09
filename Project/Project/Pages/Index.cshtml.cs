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
        public string Query { get; set; }
        public NobelLaureates NobelLaureates { get; set; }

        public static SelectList selectListItems;

        public static Dictionary<string, string> countryDictionary;

        public IActionResult OnGet(string query)
        {
            if(selectListItems == null || !selectListItems.Any())
            {
                using (var webClient = new WebClient())
                {
                    string jsonString = webClient.DownloadString("https://pkgstore.datahub.io/core/country-list/data_json/data/8c458f2d15d9f2119654b29ede6e45b8/data_json.json");

                    selectListItems = new SelectList(Country.FromJson(jsonString), "Code", "Name");
                    
                }
            } 

            ViewData["Code"] = selectListItems;

            if (!string.IsNullOrWhiteSpace(query))
            {
                if (countryDictionary == null || !countryDictionary.Any())
                {
                    countryDictionary = selectListItems.ToDictionary(x => x.Value, x => x.Text);
                }

                Query = countryDictionary[query];
            }

            return Page();
        }
    }
}
