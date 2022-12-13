using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class PrestamosModel
    {
        [Required]
        public int Monto { get; set; }

        [Required]
        public int Plazo { get; set; }
    }
}