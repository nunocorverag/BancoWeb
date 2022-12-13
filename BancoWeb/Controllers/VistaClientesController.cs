using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BancoWeb.Models;
using BancoWeb.ViewModel;
using static BancoWeb.ViewModel.VistaClientesModel;
using static System.Math;

namespace BancoWeb.Controllers
{
    public class VistaClientesController : Controller
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

        //Comprobar si el usuario es valido (Si existe uno loggeado y si es cliente)
        public bool ValidUser()
        {
            int UserID = GetUserID();
            if (UserID == -1)
            {
                return false;
            }
            cuenta checkCuenta = db.cuenta
                .Where(a => a.ID_Usuario == UserID)
                .SingleOrDefault();
            if (checkCuenta == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // GET: VistaClientes
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
                .Where(c => c.cuenta.ID_Usuario == UserID)
                .ToList();

            tablas.ListPrestamo = db.prestamo
                .Include(s => s.cuenta)
                .Where(c => c.cuenta.ID_Usuario == UserID)
                .ToList();

            tablas.cuenta = db.cuenta
                .Where(c => c.ID_Usuario == UserID)
                .SingleOrDefault();

            tablas.usuario = db.usuario
                .Where(u => u.ID_Usuario == UserID)
                .SingleOrDefault();

            return View(tablas);
        }

        // GET: VistaClientes/CrearSolicitud
        public ActionResult CrearSolicitud()
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: VistaClientes/CrearSolicitud
        [HttpPost]
        public ActionResult CrearSolicitud(solicitud solicitudModel)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                int UserID = GetUserID();

                cuenta cuentaact = db.cuenta
                    .Where(c => c.ID_Usuario == UserID)
                    .SingleOrDefault();

                solicitud solicitudCheck = db.solicitud
                    .Where(s => s.Numero_Cuenta == cuentaact.Numero_Cuenta)
                    .Where(s => s.Situacion == "Solicitado")
                    .SingleOrDefault();


                prestamo prestamoCheck = db.prestamo
                    .Where(p => p.Numero_Cuenta == cuentaact.Numero_Cuenta)
                    .Where(p => p.Estado == "Activo")
                    .SingleOrDefault();

                if (solicitudCheck != null)
                {
                    ViewBag.Message = "ERROR, ya tiene una solicitud de prestamo!";
                    return View(solicitudModel);
                }

                if (prestamoCheck != null)
                {
                    ViewBag.Message = "ERROR, Ya tiene un prestamo Activo!!";
                    return View(solicitudModel);
                }
                if (false) ;
                else
                {
                    solicitud solinfo = new solicitud();

                    cuenta cuentainfo = db.cuenta
                        .Where(u => u.ID_Usuario == UserID)
                        .SingleOrDefault();

                    solinfo.Numero_Cuenta = cuentainfo.Numero_Cuenta;
                    solinfo.Monto = solicitudModel.Monto;
                    solinfo.Fecha_Solicitud = DateTime.Now.Date;
                    solinfo.Situacion = "Solicitado";

                    solinfo.Plazo = solicitudModel.Plazo;

                    db.solicitud.Add(solinfo);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return View(solicitudModel);
        }

        // POST: VistaClientes/Pagar/[id]
        [HttpPost]
        public ActionResult Pagar(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            prestamo prestamo = db.prestamo.Find(id);

            if (prestamo == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                if (false)
                { }
                else
                {
                    int UserID = GetUserID();

                    prestamo.cuenta.Saldo = prestamo.cuenta.Saldo - prestamo.Pago_Mensual;

                    prestamo.Fecha_Ultimo_Pago = DateTime.Now;

                    cuenta cuentainf = db.cuenta
                        .Where(c => c.ID_Usuario == UserID)
                        .FirstOrDefault();

                    solicitud solicitudinf = db.solicitud
                        .Where(c => c.ID_Solicitud == prestamo.ID_Solicitud)
                        .SingleOrDefault();

                    decimal plazo = (decimal)solicitudinf.Plazo;

                    decimal porcentaje = 100 / plazo;

                    decimal porcentaje100 = Round(porcentaje, 2);

                    porcentaje100 = porcentaje100 * plazo;

                    decimal porcentajeCompara = prestamo.Porcentaje_Pagado + Round(porcentaje, 2);

                    if (porcentaje100 == porcentajeCompara)
                    {
                        prestamo.Porcentaje_Pagado = 100M;
                        prestamo.Estado = "Finalizado";
                    }
                    else
                    {
                        //Aqui hay un error
                        prestamo.Porcentaje_Pagado = prestamo.Porcentaje_Pagado + porcentaje;
                    }
                    db.prestamo.AddOrUpdate(prestamo);
                    ViewBag.Message = solicitudinf;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
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
