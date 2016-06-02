using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoFinal_DBD.Models
{
    /// <summary>
    /// Modelo de información de cuenta
    /// </summary>
    public class CuentaModel
    {
        /// <summary>
        /// Numero de cuenta asociado al cuentahabiente.
        /// </summary>
        public String Cuenta { get; set; }

        /// <summary>
        /// Nombres del cuentahabiente.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} debe llenarse")]
        [Display(Name = "Nombre")]
        [MaxLength(30, ErrorMessage = "El campo {0} debe ser menor a {1} caracteres")]
        public String Nombre { get; set; }

        /// <summary>
        /// Apellidos del cuentahabiente.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} debe llenarse")]
        [Display(Name = "Apellido")]
        [MaxLength(30, ErrorMessage = "El campo {0} debe ser menor a {1} caracteres")]
        public String Apellido { get; set; }

        /// <summary>
        /// Saldo de la cuenta.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} debe llenarse")]
        [Display(Name = "Saldo")]
        [Range(0, 999999999.99, ErrorMessage = "El campo {0} debe estar entre 0 y 999,999,999.99")]
        [RegularExpression("^([0-9])+(.[0-9]{1,2})?$", ErrorMessage = "El campo {0} debe llenarse con el formato 0.00")]
        public Decimal Saldo { get; set; }

        /// <summary>
        /// Porcentaje de interes aplicable a la cuenta.
        /// </summary>
        [Required(ErrorMessage = "El campo {0} debe llenarse")]
        [Display(Name = "Interés")]
        [Range(0, 0.99, ErrorMessage = "El campo {0} debe estar entre 0 y 0.99")]
        [RegularExpression("^0(.[0-9]{1,2})?$", ErrorMessage = "El formato del campo {0} debe ser 0.00")]
        public Decimal Interes { get; set; }

        /// <summary>
        /// Indicador de actividad de la cuenta.
        /// </summary>
        public String Status { get; set; }
    }
}