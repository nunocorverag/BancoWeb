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
using static System.Math;
using static BancoWeb.ViewModel.VistaGerentesModel;

namespace BancoWeb.Controllers
{
    public class VistaGerentesController : Controller
    {
        //Conexion a la base de datos
        private bancoEntities db = new bancoEntities();

        // Obtener el ID del usuario
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

        //Comprobar si el usuario es valido (Si existe uno loggeado y si es gerente)
        public bool ValidUser()
        {
            int UserID = GetUserID();
            if (UserID == -1)
            {
                return false;
            }
            gerente checkGerente = db.gerente
                .Where(a => a.ID_Usuario == UserID)
                .SingleOrDefault();
            if (checkGerente == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // GET: VistaGerentes
        public ActionResult Index()
        {
            if(ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            int UserID = GetUserID();

            Tablas tablas = new Tablas();

            tablas.ListCurp_Estatus = db.curp_estatus
                .Include(c => c.info_persona)
                .ToList();

            tablas.ListCuenta = db.cuenta
                .Include(c => c.usuario)
                .Include(c => c.info_persona)
                .Include(c => c.curp_estatus)
                .ToList();

            tablas.ListEmpleado = db.empleado
                .Include(e => e.usuario)
                .Include(e => e.info_persona)
                .ToList();

            tablas.ListSolicitud = db.solicitud
                .Include(s => s.cuenta)
                .Where(c => c.cuenta.ID_Usuario == UserID)
                .ToList();

            tablas.ListPrestamo = db.prestamo
                .Include(s => s.cuenta)
                .Where(c => c.cuenta.ID_Usuario == UserID)
                .ToList();

            tablas.usuario = db.usuario
                .Where(u => u.ID_Usuario == UserID)
                .SingleOrDefault();

            tablas.gerente = db.gerente
                .Include(g => g.info_persona)
                .Where(c => c.ID_Usuario == UserID)
                .SingleOrDefault();


            return View(tablas);
        }

        // POST: VistaGerentes/Aceptar
        [HttpPost]
        public ActionResult Aceptar(string id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            curp_estatus curp_estatus = db.curp_estatus.Find(id);
            if (curp_estatus == null)
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
                    usuario infous = new usuario();
                    cuenta cuentaus = new cuenta();

                    curp_estatus.Estatus = "aceptado";
                    string FechaNacimiento = curp_estatus.info_persona.Fecha_Nacimiento.ToString();
                    string Nombre = curp_estatus.info_persona.Nombre;
                    string PrimerNombre = "";
                    for (int i = 0; Nombre[i] != ' '; i++)
                    {
                        //Solo tomaremos el primer nombre para hacer el nombre de usuario
                        PrimerNombre += Nombre[i];
                    }
                    string UserName =
                        PrimerNombre
                        + curp_estatus.info_persona.Ape_P.Substring(0, 2)
                        + curp_estatus.info_persona.Ape_M.Substring(0, 2);


                    string Password =
                        curp_estatus.info_persona.Nombre.Substring(0, 2)
                        + curp_estatus.info_persona.Ape_P.Substring(0, 2)
                        + curp_estatus.info_persona.Ape_M.Substring(0, 2)
                        + FechaNacimiento.Substring(0, 2);

                    infous.UserName = UserName;
                    infous.Password = Password;

                    //Generar el numero de cuenta random entre 1 y 10 digitos
                    Random digits8 = new Random();
                    Random multiply = new Random();
                    Random add = new Random();

                    long randomNumber = digits8.Next(1, 999999999) * multiply.Next(1, 100) + add.Next(0, 99);
                    randomNumber = Abs(randomNumber);

                    cuenta check = db.cuenta.Where(cuenta => cuenta.Numero_Cuenta == randomNumber)
                    .SingleOrDefault();

                    //Checamos que el numero random no se repita
                    while (check != null)
                    {
                        randomNumber = digits8.Next(1, 999999999) * multiply.Next(1, 100) + add.Next(0, 99);
                        randomNumber = Abs(randomNumber);
                        check = db.cuenta.Where(cuenta => cuenta.Numero_Cuenta == randomNumber)
                        .SingleOrDefault();
                    }

                    cuentaus.Numero_Cuenta = randomNumber;

                    int infousID = infous.ID_Usuario;
                    cuentaus.ID_Usuario = infousID;

                    cuentaus.Saldo = 10000.00M;

                    string CURPID = curp_estatus.CURP;
                    cuentaus.CURP = CURPID;



                    //Agarramos la info de la persona
                    int infoID = curp_estatus.ID_Persona;
                    info_persona infoper = db.info_persona.Find(infoID);


                    cuentaus.ID_Persona = infoID;

                    db.usuario.Add(infous);
                    db.cuenta.Add(cuentaus);

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }
            ViewBag.ID_Persona = new SelectList(db.info_persona, "ID_Persona", "Nombre", curp_estatus.ID_Persona);
            return View(curp_estatus);
        }

        // POST: VistaGerentes/Rechazar
        [HttpPost]
        public ActionResult Rechazar(string id)
        {
            if (ValidUser() == false)
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            curp_estatus curp_estatus = db.curp_estatus.Find(id);
            if (curp_estatus == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                curp_estatus.Estatus = "rechazado";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Persona = new SelectList(db.info_persona, "ID_Persona", "Nombre", curp_estatus.ID_Persona);
            return View(curp_estatus);
        }

        // GET: VistaGerentes/CrearEmpleado
        public ActionResult CrearEmpleado()
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

        // POST: VistaGerentes/CrearEmpleado
        [HttpPost]
        public ActionResult CrearEmpleado(CrearEmpleadoModel registerDetails)
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
                empleado empleadolog = new empleado();

                infolog.Nombre = registerDetails.Nombre;
                infolog.Ape_P = registerDetails.Ape_P;
                infolog.Ape_M = registerDetails.Ape_M;
                infolog.Fecha_Nacimiento = registerDetails.Fecha_Nacimiento;

                userlog.UserName = registerDetails.UserName;
                userlog.Password = registerDetails.Password;


                db.info_persona.Add(infolog);
                db.usuario.Add(userlog);
                db.nomina.Add(nomina1);
                db.empleado.Add(empleadolog);
                db.SaveChanges();

                ViewBag.Message = "Empleado creado con exito!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(registerDetails);
            }
        }

        // GET: VistaGerentes/BorrarEmpleado
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

        // POST: VistaGerentes/BorrarEmpleado
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


        // GET: VistaGerentes/VerSolicitudes
        public ActionResult VerSolicitudes()
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

            return View(tablas);
        }


        // POST: VistaGerentes/Aceptar
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
                    if (solicitud.Plazo == 6)
                    {
                        tasadeInteres = 0.12M;
                    }
                    else if (solicitud.Plazo == 12)
                    {
                        tasadeInteres = 0.18M;
                    }
                    else if (solicitud.Plazo == 24)
                    {
                        tasadeInteres = 0.279M;
                    }
                    else if (solicitud.Plazo == 36)
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

                    empleadoinf.Prestamos_Aprobados++;

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

        // POST: VistaGerentes/Denegar
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
            if (ModelState.IsValid)
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
