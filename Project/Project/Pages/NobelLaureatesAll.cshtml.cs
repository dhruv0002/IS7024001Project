using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using QuickTypeNobelLaureates;

namespace Project.Pages
{
    public class NobelLaureateByCountryModel : PageModel
    {
       public JsonResult OnGet()
        {
            NobelLaureates nobelLaureates = null;
            using (var webClient = new WebClient())
            {
                string jsonString = "";
                JSchema schema = null;
                JObject jsonObject = null;

                try
                {
                    jsonString = webClient.DownloadString($"https://api.nobelprize.org/v1/laureate.json");
                    schema = JSchema.Parse(System.IO.File.ReadAllText("./Schemas/nobel-laureate-country-json-schema.json"));
                    jsonObject = JObject.Parse(jsonString);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: ", e);
                }

                IList<string> validationEvents = new List<string>();

                nobelLaureates = NobelLaureates.FromJson(jsonString);
                
                if (jsonObject == null || schema == null || !jsonObject.IsValid(schema, out validationEvents))
                {
                    foreach (string evt in validationEvents)
                    {
                        Console.WriteLine(evt);
                    }
                }
            }

            return new JsonResult(nobelLaureates);
        }
    }
}
