using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using dotnet_practise.Models;
using System.IO;
using Newtonsoft.Json;
using dotnet_practise.Services;

namespace dotnet_practise.Controllers
{
    public class KosaricaController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public KosaricaController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Baza.dodajEvent("Kosarica");
            IList<Proizvod> items = new List<Proizvod>(); //lista svih proizvoda

            IList<string> stanje = new List<string>(); //lista stringova s imenima proizvoda u kosarici

            IList<Proizvod> stanjePr = new List<Proizvod>(); //lista proizvoda u kosarici


            stanje = Baza.dohvatiKosaricuZaKorisnika();

            using (StreamReader r = new StreamReader("./samples/items.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<Proizvod>>(json);
            }


            foreach (var it in stanje)
            {
                foreach (var p in items)
                {
                    if (it.Equals(p.Name))
                    {
                        stanjePr.Add(p);
                        break;
                    }
                }
            }

            ViewData["items"] = stanjePr;
            ViewData["brojProizvoda"] = stanjePr.Count;
            Globals.brPr = stanjePr.Count;
            Console.WriteLine("proizvodi u kosarici: {0}", ViewData["brojProizvoda"]);
            
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult obrisiProizvod(string ime)
        {
            Baza.dodajEvent("remove", ime);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult kupi()
        {
            Baza.dodajEvent("final");
            return RedirectToAction("Index", "Aplikacija");


        }
    }
}
