using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

using Oracle.DataAccess.Client;

namespace ProyectoFinal_DBD.Helpers
{
    public class OracleConn
    {
        /// <summary>
        /// Objeto de conexión de la clase
        /// </summary>
        private Oracle.DataAccess.Client.OracleConnection conn = new Oracle.DataAccess.Client.OracleConnection();

        /// <summary>
        /// Abre la conexión con la base de datos Oracle
        /// </summary>
        /// <returns>Valor booleano indicando si la operación tuvo éxito o no</returns>
        private bool Abrir()
        {
            try
            {
                // Obtiene cadena de conexión del web.config e intenta abrir la conexión
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
                conn.Open();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una sentencia en la base de datos Oracle
        /// </summary>
        /// <param name="com">Sentencia que se desea ejecutar</param>
        /// <returns>Numero de filas afectadas</returns>
        public int ExecuteNonQueryOracle(String com)
        {
            // Ejecuta la sentencia y devuelve el numero de filas afectadas
            int rowsAffected = 0;
            try
            {
                this.Abrir();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = com;
                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return 0;
            }
            this.Cerrar();
            return rowsAffected;
        }

        /// <summary>
        /// Ejecuta una consulta en la base de datos Oracle
        /// </summary>
        /// <param name="com">Consulta que se desea ejecutar</param>
        /// <returns>DataTable con toda la información obtenida de la consulta</returns>
        public DataTable ExecuteQuery(String com)
        {
            // Ejecuta la consulta y llena el DataTable con la información obtenida
            DataTable result = new DataTable();
            try
            {
                this.Abrir();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = com;
                OracleDataReader reader = command.ExecuteReader();
                result.Load(reader);
            }
            catch (Exception e)
            {
                return null;
            }
            this.Cerrar();

            return result;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con cualquier cantidad de parametros.
        /// </summary>
        /// <param name="procedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="parametros">Listado de parametros que requiere el procedimiento almacenado.</param>
        /// <returns>Mensaje devuelto por la ejecución del procedimiento en la base de datos.</returns>
        public String ExecuteProcedure(String procedimiento, List<OracleParameter> parametros)
        {
            String resultado = String.Empty;
            try
            {
                this.Abrir();
                using (OracleCommand cmd = new OracleCommand(procedimiento, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Añade el primer parametro de retorno de resultados
                    OracleParameter parametro = new OracleParameter();
                    parametro.Direction = ParameterDirection.ReturnValue;
                    parametro.OracleDbType = OracleDbType.NVarchar2;
                    parametro.Size = 5000;
                    cmd.Parameters.Add(parametro);

                    // Añade el resto de parametros enviados en la llamada del metodo
                    foreach (OracleParameter param in parametros)
                    {
                        cmd.Parameters.Add(param);
                    }

                    // Ejecuta el procedimiento y guarda el mensaje devuelto por la ejecución
                    cmd.ExecuteNonQuery();
                    resultado = cmd.Parameters[0].Value.ToString();
                }
            }
            catch (Exception e)
            {
                this.Cerrar();
                return String.Format("Error al ejecutar transacción : {0}", e.Message);
            }
            this.Cerrar();
            return resultado;
        }

        /// <summary>
        /// Cierra la conexión con la base de datos Oracle
        /// </summary>
        /// <returns>Valor booleano indicando si la operación tuvo éxito o no</returns>
        private bool Cerrar()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}