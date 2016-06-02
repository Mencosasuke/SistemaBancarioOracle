using ProyectoFinal_DBD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ProyectoFinal_DBD.Helpers
{
    public class ModelMapper
    {
        /// <summary>
        /// Retorna el listado de cuentas en la base de datos como un listado de modelos de cuenta.
        /// </summary>
        /// <param name="cuentas"><see cref="System.Data.DataTable"/> que contiene todos los resultados de las cuentas almacenadas en la base de datos.</param>
        /// <returns>Listado de <see cref="ProyectoFinal_DBD.Models.CuentaModel"/> con la información respectiva de cada cuenta almacenda en la base de datos.</returns>
        public List<CuentaModel> MapearModeloCuenta(DataTable cuentas)
        {
            List<CuentaModel> listaCuentas = new List<CuentaModel>();

            foreach (DataRow cuenta in cuentas.Rows)
            {
                CuentaModel nuevaCuenta = new CuentaModel();
                nuevaCuenta.Cuenta = cuenta["CUENTA"] as String;
                nuevaCuenta.Nombre = cuenta["NOMBRE"] as String;
                nuevaCuenta.Apellido = cuenta["APELLIDO"] as String;
                nuevaCuenta.Saldo = Convert.ToDecimal(cuenta["SALDO"]);
                nuevaCuenta.Interes = Convert.ToDecimal(cuenta["INTERES"]);
                nuevaCuenta.Status = cuenta["STATUS"] as String;

                listaCuentas.Add(nuevaCuenta);
            }

            return listaCuentas;
        }
    }
}