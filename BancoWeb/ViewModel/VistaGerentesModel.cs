using BancoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoWeb.ViewModel
{
    public class VistaGerentesModel
    {
        public class Tablas
        {
            public List<curp_estatus> ListCurp_Estatus { get; set; }

            public List<cuenta> ListCuenta { get; set; }

            public List<empleado> ListEmpleado { get; set; }

            public List<solicitud> ListSolicitud { get; set; }

            public List<prestamo> ListPrestamo { get; set; }

            public gerente gerente {  get; set; }

            public usuario usuario { get; set; }
        }
    }
}