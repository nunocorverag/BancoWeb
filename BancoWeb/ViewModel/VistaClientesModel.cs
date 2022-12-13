using BancoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class VistaClientesModel
    {
        public class Tablas
        {
            public List<solicitud> ListSolicitud { get; set; }

            public List<prestamo> ListPrestamo { get; set; }

            public cuenta cuenta { get; set; }

            public usuario usuario { get; set; }
        }
    }
}