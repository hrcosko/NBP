using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Newtonsoft.Json;

using dotnet_practise.Services;
using dotnet_practise.Models;



namespace dotnet_practise.Controllers
{
    public class AplikacijaController : Controller
    {
        
        // 
        // GET: /HelloWorld/

        public IActionResult Index()
        {
            IList<String> itemList = new List<String>();
            var streamEvents = Baza.procitajSveEvente();

            foreach (var evt in streamEvents)
                itemList.Add(Encoding.UTF8.GetString(evt.Event.Data));
                
            ViewData["items"] = itemList;
            return View();
        }

        // 
        // GET: /HelloWorld/Welcome/ 

        public string Welcome(string name, int ID = 1)
        {
            //Baza.napuniStream();
            
            return HtmlEncoder.Default.Encode($"Hello {name}, ID: {ID}");

        }

    }
}
