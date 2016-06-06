using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProyectoFinal_DBD.Helpers;
using ProyectoFinal_DBD.Models;
using Oracle.DataAccess.Client;
using System.Data;

namespace ProyectoFinal_DBD.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Objeto de conexión a base de datos Oracle
        /// </summary>
        private OracleConn oracleCon;

        private ModelMapper mapper = new ModelMapper();

        /// <summary>
        /// Muestra el listado de cuentas del sistema
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            // Obtiene el listado de cuentas del sistema
            oracleCon = new OracleConn();
            DataTable dtCuentas = oracleCon.ExecuteQuery("SELECT * FROM SYSBANC.CUENTA");
            List<CuentaModel> listaCuentas = mapper.MapearModeloCuenta(dtCuentas);

            ViewBag.ActiveHome = "active";
            return View(listaCuentas);
        }

        /// <summary>
        /// Muestra la vista para guardar una nueva cuenta
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult NuevaCuenta()
        {
            ViewBag.ActiveNew = "active";
            return View(new CuentaModel());
        }

        /// <summary>
        /// Guarda la nueva cuenta ingresada y devuelve la vista principal
        /// </summary>
        /// <param name="cuentaModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NuevaCuenta(CuentaModel cuentaModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveNew = "active";
                return View(cuentaModel);
            }

            Guid value = Guid.NewGuid();
            cuentaModel.Cuenta = value.ToString().Substring(0, 11).Replace("-", "");

            oracleCon = new OracleConn();

            List<OracleParameter> listaParametros = new List<OracleParameter>();

            OracleParameter parametro = new OracleParameter();

            parametro = new OracleParameter("vl_cuenta", OracleDbType.NVarchar2, cuentaModel.Cuenta, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_nombre", OracleDbType.NVarchar2, cuentaModel.Nombre, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_apellido", OracleDbType.NVarchar2, cuentaModel.Apellido, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_saldo", OracleDbType.Decimal, cuentaModel.Saldo, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_interes", OracleDbType.Decimal, cuentaModel.Interes, ParameterDirection.Input);
            listaParametros.Add(parametro);

            String resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.insert_account", listaParametros);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Muestra la vista para modificar una cuenta existente
        /// </summary>
        /// <param name="cuentaModel">Datos de la cuenta a modificar</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ModificarCuenta(CuentaModel cuentaModel)
        {
            return View(cuentaModel);
        }

        /// <summary>
        /// Realiza la modificació de la cuenta alterada
        /// </summary>
        /// <param name="cuentaModel">Datos nuevos de la cuenta que se va a modificar</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateCuenta(CuentaModel cuentaModel)
        {
            if (!ModelState.IsValid)
            {
                return View(cuentaModel);
            }

            oracleCon = new OracleConn();

            List<OracleParameter> listaParametros = new List<OracleParameter>();

            OracleParameter parametro = new OracleParameter();

            parametro = new OracleParameter("vl_cuenta", OracleDbType.NVarchar2, cuentaModel.Cuenta, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_nombre", OracleDbType.NVarchar2, cuentaModel.Nombre, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_apellido", OracleDbType.NVarchar2, cuentaModel.Apellido, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_saldo", OracleDbType.Decimal, cuentaModel.Saldo, ParameterDirection.Input);
            listaParametros.Add(parametro);
            parametro = new OracleParameter("vl_interes", OracleDbType.Decimal, cuentaModel.Interes, ParameterDirection.Input);
            listaParametros.Add(parametro);

            String resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.update_account", listaParametros);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Deshabilita una cuenta del sistema
        /// </summary>
        /// <param name="cuenta">Código de la cuenta a deshabilitar</param>
        /// <returns></returns>
        public ActionResult DeshabilitarCuenta(String cuenta)
        {
            String resultado = String.Empty;
            if (cuenta != null)
            {
                oracleCon = new OracleConn();
                List<OracleParameter> listaParametros = new List<OracleParameter>();
                OracleParameter parametro = new OracleParameter();
                parametro = new OracleParameter("vl_cuenta", OracleDbType.NVarchar2, cuenta, ParameterDirection.Input);
                listaParametros.Add(parametro);
                resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.delete_account", listaParametros);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Habilitar una cuenta del sistema
        /// </summary>
        /// <param name="cuenta">Código de la cuenta a habilitar</param>
        /// <returns></returns>
        public ActionResult HabilitarCuenta(String cuenta)
        {
            String resultado = String.Empty;
            if (cuenta != null)
            {
                oracleCon = new OracleConn();
                List<OracleParameter> listaParametros = new List<OracleParameter>();
                OracleParameter parametro = new OracleParameter();
                parametro = new OracleParameter("vl_cuenta", OracleDbType.NVarchar2, cuenta, ParameterDirection.Input);
                listaParametros.Add(parametro);
                resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.reopen_account", listaParametros);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Renderiza la vista para ingresar los datos de la transferencia
        /// </summary>
        /// <param name="cuenta">Numero de cuenta de origen</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Transferencia(String cuenta)
        {
            // Obtiene el listado de cuentas del sistema
            List<CuentaModel> listaCuentas = new List<CuentaModel>();
            oracleCon = new OracleConn();
            DataTable dtCuentas = oracleCon.ExecuteQuery(String.Format("SELECT * FROM SYSBANC.CUENTA WHERE STATUS = 'X' AND CUENTA <> '{0}' ORDER BY NOMBRE ASC", cuenta));
            listaCuentas = mapper.MapearModeloCuenta(dtCuentas);
            
            ViewBag.ListaCuentas = listaCuentas;
            ViewBag.Cuenta = cuenta;

            return View();
        }

        /// <summary>
        /// Realiza la transferencia de montos entre la cuenta origen y la cuenta destino
        /// </summary>
        /// <param name="cuenta">No. de cuenta origen</param>
        /// <param name="cuentaDestino">No. de cuenta destino</param>
        /// <param name="monto">Cantidad de dinero a transferir de una cuenta a otra</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Transferencia(String cuenta, String cuentaDestino, Decimal monto)
        {
            String resultado = String.Empty;
            if (cuenta != null && cuentaDestino != null && monto >= 0)
            {
                oracleCon = new OracleConn();
                List<OracleParameter> listaParametros = new List<OracleParameter>();
                OracleParameter parametro = new OracleParameter();
                parametro = new OracleParameter("vl_cuentaOrigen", OracleDbType.NVarchar2, cuenta, ParameterDirection.Input);
                listaParametros.Add(parametro);
                parametro = new OracleParameter("vl_cuentaDestino", OracleDbType.NVarchar2, cuentaDestino, ParameterDirection.Input);
                listaParametros.Add(parametro);
                parametro = new OracleParameter("vl_monto", OracleDbType.Decimal, monto, ParameterDirection.Input);
                listaParametros.Add(parametro);
                resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.transf_saldos", listaParametros);
            }
            else
            {
                // Devolver error en campos
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retorna la vista para realizar un aumento a todas las cuentas del sistema
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult IncrementarLote()
        {
            ViewBag.ActiveInc = "active";
            return View();
        }

        /// <summary>
        /// Realiza un incremento en el saldo de todas las cuentas activas
        /// </summary>
        /// <param name="cantidad">Monto a incrementar en las cuentas del sistema</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult IncrementarCuentas(Decimal monto)
        {
            String resultado = String.Empty;
            if (monto >= 0)
            {
                oracleCon = new OracleConn();
                List<OracleParameter> listaParametros = new List<OracleParameter>();
                OracleParameter parametro = new OracleParameter();
                parametro = new OracleParameter("vl_monto", OracleDbType.Decimal, monto, ParameterDirection.Input);
                listaParametros.Add(parametro);
                resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.aumentar_saldos", listaParametros);
            }
            else
            {
                // Mensaje de error
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retorna la vista para realizar un decremento a todas las cuentas del sistema
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DecrementarLote()
        {
            ViewBag.ActiveDec = "active";
            return View();
        }

        /// <summary>
        /// Realiza un decremento en el saldo de todas las cuentas activas
        /// </summary>
        /// <param name="cantidad">Monto a decrementar en las cuentas del sistema</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DecrementarCuentas(Decimal monto)
        {
            String resultado = String.Empty;
            if (monto >= 0)
            {
                oracleCon = new OracleConn();
                List<OracleParameter> listaParametros = new List<OracleParameter>();
                OracleParameter parametro = new OracleParameter();
                parametro = new OracleParameter("vl_monto", OracleDbType.Decimal, monto, ParameterDirection.Input);
                listaParametros.Add(parametro);
                resultado = oracleCon.ExecuteProcedure("SYSBANC.pkg_account_management.decrementar_saldos", listaParametros);
            }
            else
            {
                // Mensaje de error
            }
            return RedirectToAction("Index");
        }
    }
}