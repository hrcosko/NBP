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
            IList<Proizvod> items = new List<Proizvod>();

            using (StreamReader r = new StreamReader("./samples/items.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<Proizvod>>(json);
            }

            ViewData["items"] = items;
            //bool property za boju buttona
            return View();
        }

        // 
        // GET: /HelloWorld/Welcome/ 

        public string Welcome(string name, int ID = 1)
        {
            
            return HtmlEncoder.Default.Encode($"Hello {name}, ID: {ID}");

        }

        [HttpPost]
        public ActionResult dodajProizvod(string ime, string cijena)
        {
            Baza.dodajEvent("add", ime, cijena);
            return RedirectToAction("Index");
        }
    }
}
