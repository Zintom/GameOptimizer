using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zintom.GameOptimizer.Menus
{
    interface IConsoleMenu
    {

        /// <summary>
        /// Runs the given menu.
        /// </summary>
        public void Run(InteractiveShell.InteractiveShell gui);

    }
}
