using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using BancoWeb.Models;
using BancoWeb.ViewModel;

namespace BancoWeb.Controllers
{
    public class AccountController : Controller
    {
        //QUITAR MSG CUANDO TODO SALGA
        public static string Msg = "";
        // GET: ApplicationContext

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }


        #region Registro del usuario
        [HttpPost]
        public ActionResult RegisterRequest(RegisterModel registerDetails)
        {
            if (ModelState.IsValid)
            {
                using (var databaseContext = new bancoEntities())
                {

                    curp_estatus check = databaseContext.curp_estatus
                    .Where(query => query.CURP.Equals(registerDetails.Curp))
                    .SingleOrDefault();

                    #region Checar si el usuario ya existe
                    if (check != null)
                    {
                        ViewBag.Message = "ERROR, El Usuario ya existe!";
                    }
                    #endregion

                    else
                    {
                        #region Checar si contiene un numero
                        //RQNF 1
                        for (int i = 0; i < registerDetails.Nombre.Length; i++)
                        {
                            if (Char.IsDigit(registerDetails.Nombre, i))
                            {
                                ViewBag.Message = "ERROR, el Nombre no puede contener numeros!!";
                                return View("Register", registerDetails);
                            }
                        }
                        for (int i = 0; i < registerDetails.Ape_P.Length; i++)
                        {
                            if (Char.IsDigit(registerDetails.Ape_P, i))
                            {
                                ViewBag.Message = "ERROR, el Apellido Paterno no puede contener numeros!!";
                                return View("Register", registerDetails);
                            }

                        }
                        for (int i = 0; i < registerDetails.Ape_M.Length; i++)
                        {
                            if (Char.IsDigit(registerDetails.Ape_M, i))
                            {
                                ViewBag.Message = "ERROR, el Apellido Materno no puede contener numeros!!";
                                return View("Register", registerDetails);
                            }

                        }
                        #endregion

                        #region Checar si la fehca es antes de 1962
                        //RQNF 2
                        if (registerDetails.Fecha_Nacimiento.Year < 1962)
                        {
                            ViewBag.Message = "ERROR, la fecha de nacimiento no puede ser antes de 1962!!";
                            return View("Register", registerDetails);

                        }
                        #endregion

                        #region Checar si el CURP es correcto, si lo es, crear el usuario
                        if (registerDetails.Curp.Length < 18)
                        {
                            ViewBag.Message = "ERROR, el curp no puede ser menor a 18 caracteres!";
                            return View("Register", registerDetails);
                        }
                        else if (registerDetails.Curp.Length > 18)
                        {
                            ViewBag.Message = "ERROR, el curp no puede ser mayor a 18 caracteres!";
                            return View("Register", registerDetails);
                        }
                        else if (registerDetails.Curp.Length == 18)
                        {
                            //RQNF 3
                            if (ComprobarCurp(registerDetails.Curp, registerDetails.Nombre, registerDetails.Ape_P, registerDetails.Ape_M, registerDetails.Fecha_Nacimiento))
                            {
                                info_persona infolog = new info_persona();
                                curp_estatus curplog = new curp_estatus();

                                infolog.Nombre = registerDetails.Nombre;
                                infolog.Ape_P = registerDetails.Ape_P;
                                infolog.Ape_M = registerDetails.Ape_M;
                                infolog.Fecha_Nacimiento = registerDetails.Fecha_Nacimiento.Date;
                                curplog.CURP = registerDetails.Curp;
                                curplog.Estatus = "pendiente";

                                databaseContext.info_persona.Add(infolog);
                                databaseContext.curp_estatus.Add(curplog);
                                databaseContext.SaveChanges();

                                ViewBag.Message = "Solicitud para crear Usuario enviada, si es aprobada, recibira su nombre de Usuario y contraseña";
                            }
                            else
                            {
                                ViewBag.Message = "ERROR, El CURP no coincide con los datos ingresados!";
                                return View("Register", registerDetails);
                            }
                            #endregion

                        }
                    }
                }
                return View("Register");
            }
            else
            {
                return View("Register", registerDetails);
            }
        }
        #endregion


        public static bool ComprobarCurp(string _CURP, string _Nombre, string _Ape_P, string _Ape_M, DateTime _Fecha_Nacimiento)
        {
            //Inicial del apellido paterno
            string IAP = _Ape_P.Substring(0, 1).Replace("Ñ", "X").Replace(@"[^\u0000-\uu7F] + á,é,í,ó,ú", "X").ToUpper();
            //Vocal inicial del apellido paterno
            string VIAP = fisrtVowel(_Ape_P);
            //Inicial del apellido materno
            string IAM = _Ape_M.Substring(0, 1).Replace("Ñ", "X").Replace(@"[^\u0000-\uu7F] + á,é,í,ó,ú", "X").ToUpper();
            //Inicial del nombre
            string IN = _Nombre.Substring(0, 1).Replace("Ñ", "X").Replace(@"[^\u0000-\uu7F] + á,é,í,ó,ú", "X").ToUpper();
            string FN = _Fecha_Nacimiento.ToString("yy-MM-dd").Replace("-", "");
            Msg = IAP + VIAP + IAM + IN + FN + "/" + _CURP;
            //Curp tomado de los datos Nombre, Appellido Paterno, Apellido Materno y Fecha de Nacimiento
            //CurpC -> Curp Caculado
            string CurpC = IAP + VIAP + IAM + IN + FN;
            //Curp tomado del campo CURP cortado para compararse con CurpC
            //CurpComp Curp Comparar
            string CurpComp = _CURP.Substring(0, 10).ToUpper();

            //Parte de fecha de nacimiento CURP
            string FNCURP = _CURP.Substring(4, 6);
            if (CurpC == CurpComp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isVowel(char c)
        {
            c = Char.ToUpper(c);
            if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U')
            {
                return true;
            }
            return false;
        }
        public static string fisrtVowel(string s)
        {
            //Quitar acentos
            s = s.Replace(@"[^\u0000-\uu7F] + á,é,í,ó,ú", "X").ToUpper();
            //First Vowel
            string FV;
            for (int i = 0; i < s.Length; i++)
            {
                if (isVowel(s[i]))
                {
                    FV = Char.ToString(s[i]);
                    return FV;
                }
            }
            return "-1";
        }

        public ActionResult Buscar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Buscar(BuscarModel buscarModel)
        {
            if (ModelState.IsValid)
            {
                using (var databaseContext = new bancoEntities())
                {
                    var SEARCH = databaseContext.curp_estatus
                        .Where(c => c.CURP == buscarModel.CURP);
                    return View(SEARCH.ToList());
                }
            }
            else
            {
                return View();
            }
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(usuario model)
        {
            if (ModelState.IsValid)
            {
                var isValidUser = IsValidUser(model);

                if (isValidUser != null)
                {
                    using (var databaseContext = new bancoEntities())
                    {
                        usuario User = databaseContext.usuario
                            .Where(id => id.UserName == model.UserName)
                            .SingleOrDefault();

                        cuenta cuentaUS = databaseContext.cuenta
                            .Where(c => c.ID_Usuario == User.ID_Usuario)
                            .SingleOrDefault();

                        empleado empleadoUS = databaseContext.empleado
                            .Where(e => e.ID_Usuario == User.ID_Usuario)
                            .SingleOrDefault();

                        gerente gerenteUS = databaseContext.gerente
                            .Where(g => g.ID_Usuario == User.ID_Usuario)
                            .SingleOrDefault();

                        administrador adminUS = databaseContext.administrador
                            .Where(a => a.ID_Usuario == User.ID_Usuario)
                            .SingleOrDefault();

                        if (cuentaUS != null)
                        {
                            return RedirectToAction("Index", "VistaClientes");
                        }
                        if (empleadoUS != null)
                        {
                            return RedirectToAction("Index", "VistaEmpleados");
                        }
                        if (gerenteUS != null)
                        {
                            return RedirectToAction("Index", "VistaGerentes");
                        }
                        if (adminUS != null)
                        {
                            return RedirectToAction("Index", "VistaAdministradores");
                        }
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("Failure", "Wrong username and password combination !");
                    return View();
                }
            }
            else
            {
                return View(model);
            }
        }

        public usuario IsValidUser(usuario model)
        {
            using (var databaseContext = new bancoEntities())
            {
                usuario user = databaseContext.usuario
                .Where(query => query.UserName == model.UserName && query.Password == model.Password)
                .SingleOrDefault();

                if (user == null)
                {
                    return null;
                }
                else
                {
                    FormsAuthentication.SetAuthCookie((user.ID_Usuario).ToString(), false);
                    return user;
                }
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Lockout()
        {
            Session["Timer"] = DateTime.Now.AddMinutes(1).ToString();
            return View();
        }

        protected void timerTest_tick(object sender, EventArgs e)
        {
            if (DateTime.Compare(DateTime.Now, DateTime.Parse(Session["Timer"].ToString())) < 0)
            {
                ViewBag.Message = "Time Left : " + ((Int32)DateTime.Parse(Session["Timer"].ToString()).Subtract(DateTime.Now).TotalMinutes)
                   .ToString() + " Minute " + (((Int32)DateTime.Parse(Session["Timer"].ToString()).Subtract(DateTime.Now).TotalSeconds) % 60)
                   .ToString() + "Seconds";
            }
            else
            {
                ViewBag.Message = "Time Expire...";
            }
        }

    }
}