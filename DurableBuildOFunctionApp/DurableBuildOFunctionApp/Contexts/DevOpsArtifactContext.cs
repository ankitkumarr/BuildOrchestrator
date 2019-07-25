using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Contexts
{
    public class DevOpsArtifactContext
    {
        public string ArtifactTaskName { get; set; }

        public string Agent { get; set; }

        public string Organization { get; set; }

        public string Project { get; set; }

        public int BuildId { get; set; }

        public string ArtifactName { get; set; }
    }
}
