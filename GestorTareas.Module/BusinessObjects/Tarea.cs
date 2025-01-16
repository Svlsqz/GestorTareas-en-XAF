using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace GestorTareas.Module.BusinessObjects
{
    [DefaultClassOptions] 
    public class Tarea : BaseObject
    {
        public Tarea() : base() { }

        public virtual string Descripcion { get; set; }
        public virtual bool Completada { get; set; }
    }
}
