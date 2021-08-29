﻿using System.IO;

namespace DockerSdk.Core.Models
{
    public class GetArchiveFromContainerResponse
    {
        public ContainerPathStatResponse? Stat { get; set; }

        public Stream? Stream { get; set; }
    }
}