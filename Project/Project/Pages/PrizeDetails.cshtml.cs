using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickTypeNobelLaureates;

namespace Project.Pages
{
    public class DetailsModel : PageModel
    {
        public bool SearchCompleted { get; set; }
        public NobelLaureates NobelLaureates { get; set; }

        public IActionResult OnGet(int? id)
        {
            SearchCompleted = true;

            if (id == null)
            {
                SearchCompleted = false;
            }
            else
            {
                using (var webClient = new WebClient())
                {
                    string jsonString = "";
                    try
                    {
                        jsonString = webClient.DownloadString($"https://api.nobelprize.org/v1/laureate.json?id={id}");
                    } 
                    catch(Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                    }
                    
                    NobelLaureates = NobelLaureates.FromJson(jsonString);

                }

                if (NobelLaureates == null)
                {
                    SearchCompleted = false;
                }
            }

            return Page();
        }
    }
}
