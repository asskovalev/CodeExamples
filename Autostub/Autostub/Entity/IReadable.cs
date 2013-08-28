using System.Xml.Linq;

namespace Autostub.Entity
{
    public interface IReadable<out T>
    {
        T Read(XElement src);
    }
}
