using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meeting_console_app.CLIViews
{
    public abstract class Command
    {
        public abstract string InvocationName { get; }
        public abstract string[] Arguments { get; }
        public abstract string Description { get; }

        public abstract CLIView ExecuteCommand(CLIContext context, string[] args);
    }
}
