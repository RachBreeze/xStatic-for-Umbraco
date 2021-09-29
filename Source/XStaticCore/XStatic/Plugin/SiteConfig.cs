﻿using NPoco;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using XStatic.Generator;

namespace XStatic.Plugin
{
    public class SiteConfig : ISiteConfig
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public bool AutoPublish { get; set; }

        public int RootNode { get; set; }

        public string MediaRootNodes { get; set; }

        public string ExportFormat { get; set; }

        public DateTime? LastRun { get; set; }

        public int? LastBuildDurationInSeconds { get; set; }

        public DateTime? LastDeployed { get; set; }

        public int? LastDeployDurationInSeconds { get; set; }

        public string AssetPaths { get; set; }

        public string TargetHostname { get; set; }

        public string ImageCrops { get; set; }

        [SerializedColumn]
        public DeploymentTargetModel DeploymentTarget { get; set; }

        public IDeploymentTarget GetDeploymentTarget()
        {
            return DeploymentTarget;
        }
    }

    public class DeploymentTargetModel : IDeploymentTarget
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Fields { get; set; }
    }
}
