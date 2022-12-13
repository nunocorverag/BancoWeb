using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class BuscarModel
    {
        [Required]
        public string CURP { get; set; }
        public string Estatus {  get; set; }
    }
}