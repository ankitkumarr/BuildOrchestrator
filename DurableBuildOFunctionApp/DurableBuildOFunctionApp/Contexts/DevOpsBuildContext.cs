using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Contexts
{
    public class DevOpsBuildContext
    {
        public string BuildTaskName { get; set; }

        public string Organization { get; set; }

        public string Project { get; set; }

        public string Agent { get; set; }

        public int DefinitionId { get; set; }

        public IDictionary<string, string> Parameters { get; set; }
    }
}
