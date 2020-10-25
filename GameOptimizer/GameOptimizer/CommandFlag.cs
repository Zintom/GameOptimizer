using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOptimizer
{
    /// <summary>
    /// A key pair type class for command flags.
    /// </summary>
    public class CommandFlag
    {

        public string Flag = "";
        public string Value = "";

        public CommandFlag(string flag, string value)
        {
            this.Flag = flag;
            this.Value = value;
        }

    }
}