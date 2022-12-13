using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BancoWeb.Models;
using static BancoWeb.ViewModel.VistaEmpleadosModel;


namespace BancoWeb.Controllers
{
    public class VistaEmpleadosController : Controller
    {
        //Conexion a la base de datos
        private bancoEntities db = new bancoEntities();

        //Obtener el ID del usuario
        public int GetUserID()
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName];
            if (authCookie == null)
            {
                return -1;
            }
            else
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int UserID = int.Parse(ticket.Name);
                return UserID;
            }
        }

        //Comprobar si el usuario es valido (Si existe uno loggeado y si es empleado)
        public bool ValidUser()
        {
            int UserID = GetUserID();
            if (UserID == -1)
            {
                return false;
            }
            empleado checkEmpleado = db.empleado
                .Where(a => a.ID_Usuario == UserID)
                .SingleOrDefault();
            if (checkEmpleado == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // GET: VistaEmpleados
        public ActionResult Index()
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            int UserID = GetUserID();

            Tablas tablas = new Tablas();

            tablas.ListSolicitud = db.solicitud
                .Include(s => s.cuenta)
                .Include(s => s.cuenta.curp_estatus)
                .ToList();

            tablas.usuario = db.usuario
                 .Where(u => u.ID_Usuario == UserID)
                 .SingleOrDefault();

            tablas.empleado = db.empleado
                .Include(g => g.info_persona)
                .Where(c => c.ID_Usuario == UserID)
                .SingleOrDefault();

            return View(tablas);
        }

        // POST: VistaEmpleados/Aceptar
        [HttpPost]
        public ActionResult Aprobar(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            solicitud solicitud = db.solicitud.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                //curp_estatus checkifuserexists = db.curp_estatus.Where(c => c.CURP == curp_estatus.CURP)
                //.SingleOrDefault();
                //if (checkifuserexists != null)
                //{
                //    ViewBag.Message = "ERROR, The user is already accepted";
                //    return RedirectToAction("Index");
                //}
                if (false)
                { }
                else
                {
                    int UserID = GetUserID();

                    solicitud.Situacion = "Aprobado";

                    prestamo prestamoinf = new prestamo();

                    empleado empleadoinf = db.empleado
                        .Where(e => e.ID_Usuario == UserID)
                        .SingleOrDefault();


                    prestamoinf.Numero_Cuenta = solicitud.Numero_Cuenta;

                    empleado empleadoinfo = db.empleado
                        .Where(e => e.ID_Usuario == UserID)
                        .SingleOrDefault();

                    prestamoinf.ID_Empleado = empleadoinfo.ID_Empleado;
                    prestamoinf.Fecha_Aprobacion = DateTime.Now.Date;

                    int IDSolicitud = solicitud.ID_Solicitud;

                    prestamoinf.ID_Solicitud = IDSolicitud;

                    //Hacer la categoria del prestamo
                    //Categorias: 
                    //<5000, < 10,000, < 50,000, < 100,000, > 500,000
                    if (solicitud.Monto < 5000)
                    {
                        prestamoinf.Categoria_Prestamo = "< 5000";
                    }
                    else if (solicitud.Monto < 10000)
                    {
                        prestamoinf.Categoria_Prestamo = "< 10000";
                    }
                    else if (solicitud.Monto < 50000)
                    {
                        prestamoinf.Categoria_Prestamo = "< 50000";
                    }
                    else if (solicitud.Monto < 100000)
                    {
                        prestamoinf.Categoria_Prestamo = "< 100000";
                    }
                    else if (solicitud.Monto > 500000)
                    {
                        prestamoinf.Categoria_Prestamo = "> 500000";
                    }

                    decimal tasadeInteres = 0.0M;
                    if(solicitud.Plazo == 6)
                    {
                        tasadeInteres = 0.12M;
                    }
                    else if(solicitud.Plazo == 12)
                    {
                        tasadeInteres = 0.18M;
                    }
                    else if(solicitud.Plazo == 24)
                    {
                        tasadeInteres = 0.279M;
                    }
                    else if(solicitud.Plazo == 36)
                    {
                        tasadeInteres = 0.42M;                    //db.prestamo.Add(prestamoinf);
                    }
                    decimal interes = solicitud.Monto * tasadeInteres;

                    decimal pagoTotal = solicitud.Monto + interes;

                    decimal pagoMensual = pagoTotal / solicitud.Plazo;

                    pagoMensual = Math.Round(pagoMensual, 2);

                    prestamoinf.Pago_Mensual = pagoMensual;

                    prestamoinf.Fecha_Ultimo_Pago = default(DateTime);

                    //prestamoinf.Porcentaje_Pagado = 0M;

                    prestamoinf.Boletos = 0;

                    prestamoinf.Estado = "Activo";

                    empleadoinf.Prestamos_Aprobados ++;

                    cuenta cuentainf = db.cuenta
                        .Where(c => c.Numero_Cuenta == solicitud.Numero_Cuenta)
                        .SingleOrDefault();

                    cuentainf.Saldo += solicitud.Monto;


                    db.cuenta.AddOrUpdate(cuentainf);
                    db.prestamo.Add(prestamoinf);
                    db.empleado.AddOrUpdate(empleadoinf);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }
            return View(solicitud);
        }

        // POST: VistaEmpleados/Denegar
        [HttpPost]
        public ActionResult Denegar(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            solicitud solicitud = db.solicitud.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            if(ModelState.IsValid)
            {
                solicitud.Situacion = "Denegado";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(solicitud);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
