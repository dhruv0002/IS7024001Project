using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Breweries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project.Pages
{
    public class BreweriesAllModel : PageModel
    {
        public List<BreweryCollection> BreweryList { get; set; }
        public IActionResult OnGet()
        {
            
            using (var webClient = new WebClient())
            {
                string jsonString = "";
                try
                {
                    jsonString = webClient.DownloadString("https://breweryarc.azurewebsites.net/breweryentire");
                    BreweryList = BreweryCollection.FromJson(jsonString);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: ", e);
                }

            }

            return Page();
        }
    }
}
