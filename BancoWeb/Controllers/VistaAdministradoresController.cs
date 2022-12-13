using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BancoWeb.Models;
using BancoWeb.ViewModel;
using static BancoWeb.ViewModel.VistaAdministradoresModel;

namespace BancoWeb.Controllers
{
    public class VistaAdministradoresController : Controller
    {
        //Conexion a la base de datos
        private bancoEntities db = new bancoEntities();

        //Funcion para obtener la ID del usuario
        public int GetUserID()
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName];
            if(authCookie == null )
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

        //Comprobar si el usuario es valido (Si existe uno loggeado y si es administrador)
        public bool ValidUser()
        {
            int UserID = GetUserID();
            if (UserID == -1)
            {
                return false;
            }
            administrador checkAdministrador = db.administrador
                .Where(a => a.ID_Usuario == UserID)
                .SingleOrDefault();
            if (checkAdministrador == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // GET: VistaAdministradores
        public ActionResult Index()
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            int UserID = GetUserID();

            Tablas tablas = new Tablas();

            tablas.ListCuenta = db.cuenta
                .Include(c => c.usuario)
                .Include(c => c.info_persona)
                .Include(c => c.curp_estatus)
                .ToList();

            tablas.ListEmpleado = db.empleado
                .Include(e => e.usuario)
                .Include(e => e.info_persona)
                .ToList();

            tablas.ListGerente = db.gerente
                .Include(g => g.usuario)
                .Include(c => c.info_persona)
                .ToList();

            tablas.usuario = db.usuario
                .Where(u => u.ID_Usuario == UserID)
                .SingleOrDefault();

            tablas.administrador = db.administrador
                .Where(a => a.ID_Usuario == UserID)
                .SingleOrDefault();

            return View(tablas);
        }

        // GET: VistaAdministradores/BorrarCuenta/[id]
        public ActionResult BorrarCuenta(int? id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cuenta cuenta= db.cuenta.Find(id);
            if (cuenta == null)
            {
                return HttpNotFound();
            }
            return View(cuenta);
        }

        // POST: VistaAdministradores/BorrarCuenta/[id]
        [HttpPost, ActionName("BorrarCuenta")]
        [ValidateAntiForgeryToken]
        public ActionResult BorrarCuentaConfirmed(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            cuenta cuenta = db.cuenta.Find(id);

            int userID = cuenta.usuario.ID_Usuario;
            usuario usuario = db.usuario.Find(userID);

            int infoID = cuenta.info_persona.ID_Persona;
            info_persona info = db.info_persona.Find(infoID);

            string curp = cuenta.curp_estatus.CURP;
            curp_estatus curp_estatus = db.curp_estatus.Find(curp);

            db.cuenta.Remove(cuenta);
            db.usuario.Remove(usuario);
            db.info_persona.Remove(info);
            db.curp_estatus.Remove(curp_estatus);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: VistaAdministradores/BorrarEmpleado/[id]
        public ActionResult BorrarEmpleado(int? id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            empleado empleado = db.empleado.Find(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }
            return View(empleado);
        }

        // POST: VistaAdministradores/BorrarEmpleado/[id]
        [HttpPost, ActionName("BorrarEmpleado")]
        [ValidateAntiForgeryToken]
        public ActionResult BorrarEmpleadoConfirmed(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            empleado empleado = db.empleado.Find(id);

            int userID = empleado.usuario.ID_Usuario;
            usuario usuario = db.usuario.Find(userID);

            int infoID = empleado.info_persona.ID_Persona;
            info_persona info = db.info_persona.Find(infoID);

            int nom = empleado.nomina1.Nomina1;
            nomina nomina = db.nomina.Find(nom);

            db.empleado.Remove(empleado);
            db.usuario.Remove(usuario);
            db.info_persona.Remove(info);
            db.nomina.Remove(nomina);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: VistaAdministradores/CrearGerente/[id]
        public ActionResult CrearGerente()
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ID_Usuario = new SelectList(db.usuario, "ID_Usuario", "UserName");
            ViewBag.ID_Persona = new SelectList(db.info_persona, "ID_Persona", "Nombre");
            ViewBag.Nomina = new SelectList(db.nomina, "Nomina1", "Nomina1");
            return View();
        }

        // POST: VistaAdministradores/CrearGerente/[id]
        [HttpPost]
        public ActionResult CrearGerente(CrearEmpleadoModel registerDetails)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                info_persona infolog = new info_persona();
                usuario userlog = new usuario();
                nomina nomina1 = new nomina();
                gerente gerentelog = new gerente();

                infolog.Nombre = registerDetails.Nombre;
                infolog.Ape_P = registerDetails.Ape_P;
                infolog.Ape_M = registerDetails.Ape_M;
                infolog.Fecha_Nacimiento = registerDetails.Fecha_Nacimiento;

                userlog.UserName = registerDetails.UserName;
                userlog.Password = registerDetails.Password;


                db.info_persona.Add(infolog);
                db.usuario.Add(userlog);
                db.nomina.Add(nomina1);
                db.gerente.Add(gerentelog);
                db.SaveChanges();

                ViewBag.Message = "Gerente creado con exito!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(registerDetails);
            }
        }

        // GET: VistaAdministradores/BorrarGerente/[id]
        public ActionResult BorrarGerente(int? id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            gerente gerente = db.gerente.Find(id);
            if (gerente == null)
            {
                return HttpNotFound();
            }
            return View(gerente);
        }

        // POST: VistaAdministradores/BorrarGerente/[id]
        [HttpPost, ActionName("BorrarGerente")]
        [ValidateAntiForgeryToken]
        public ActionResult BorrarGerenteConfirmed(int id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            gerente gerente = db.gerente.Find(id);

            int userID = gerente.usuario.ID_Usuario;
            usuario usuario = db.usuario.Find(userID);

            int infoID = gerente.info_persona.ID_Persona;
            info_persona info = db.info_persona.Find(infoID);

            int nom = gerente.nomina1.Nomina1;
            nomina nomina = db.nomina.Find(nom);

            db.gerente.Remove(gerente);
            db.usuario.Remove(usuario);
            db.info_persona.Remove(info);
            db.nomina.Remove(nomina);
            db.SaveChanges();
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
