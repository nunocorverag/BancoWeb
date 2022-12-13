using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class SolicitudModel
    {
        [Required]
        public int Monto { get; set; }
                
            [Required]
            public string Plazo { get; set; }

            public string[] Plazos = new[] { "6", "12", "24", "36" };
    }
}