using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meeting_console_app.CLIViews
{
    public sealed class QuitCommand : Command
    {
        public override string InvocationName => "Q";

        public override string[] Arguments => Array.Empty<string>();

        public override string Description => "Quit the program and save changes";

        public override CLIView ExecuteCommand(CLIContext context, string[] args)
        {
            return null;
        }
    }
}
