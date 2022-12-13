using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using BancoWeb.Models;

namespace BancoWeb.ViewModel
{
    public class VistaAdministradoresModel
    {
        public class Tablas
        {
            public List<usuario> ListUsuario { get; set; }

            public List<cuenta> ListCuenta { get; set; }

            public List<empleado> ListEmpleado { get; set; }

            public List<gerente> ListGerente { get; set; }

            public administrador administrador { get; set; }

            public usuario usuario { get; set; }

        }
    }
}