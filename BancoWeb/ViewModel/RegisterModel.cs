﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BancoWeb.ViewModel
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Apellido Paterno")]
        public string Ape_P { get; set; }

        [Required]
        [Display(Name = "Apellido Materno")]
        public string Ape_M { get; set; }

        [Required]
        [Display(Name = "CURP")]
        public string Curp { get; set; }

        [Required]
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime Fecha_Nacimiento { get; set; }
    }
}