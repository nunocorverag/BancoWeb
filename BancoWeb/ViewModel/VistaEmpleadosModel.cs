using BancoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class VistaEmpleadosModel
    {
        public class Tablas
        {
            public List<solicitud> ListSolicitud { get; set; }

            public empleado empleado { get; set; }

            public usuario usuario { get; set; }
        }
    }
}