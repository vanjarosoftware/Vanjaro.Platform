using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanjaro.Core.Entities.Events
{
    public interface IVanjaroEvent
    {
        void onAction(VanjaroEventArgs e, params object[] DataObject);
    }
}
