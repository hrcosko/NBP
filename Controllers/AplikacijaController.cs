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
            Baza.dodajEvent("Aplikacija");
            IList<Proizvod> items = new List<Proizvod>();
            IList<String> preporuceno = new List<String>();
            ViewData["brojProizvoda"] = Globals.brPr;

            preporuceno = Baza.ZaPreporuku();

            using (StreamReader r = new StreamReader("./samples/items.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<Proizvod>>(json);
            }

            Globals.items = items;
            ViewData["items"] = items;
            ViewData["preporuceno"] = preporuceno;
            Console.WriteLine("proizvodi: {0}", ViewData["brojProizvoda"]);
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
            //Console.WriteLine("dodajem u kosaricu" + ime);
            Baza.dodajEvent("add", ime, cijena);
            Globals.brPr++;
            return RedirectToAction("Index");
        }
    }
}
