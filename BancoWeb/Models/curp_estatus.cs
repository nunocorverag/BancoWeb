//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BancoWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class curp_estatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public curp_estatus()
        {
            this.cuenta = new HashSet<cuenta>();
        }
    
        public string CURP { get; set; }
        public string Estatus { get; set; }
        public int ID_Persona { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cuenta> cuenta { get; set; }
        public virtual info_persona info_persona { get; set; }
    }
}
