using DurableBuildOFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Contexts
{
    public class BlobUploadContext
    {
        public string Worker { get; set; }

        public string Environment { get; set; }

        public string Platform { get; set; }

        public DevOpsArtifact Artifact { get; set; }
    }
}
