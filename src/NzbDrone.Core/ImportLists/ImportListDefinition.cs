using System;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition
    {
        public bool EnableAutomaticAdd { get; set; }
        public ImportListMonitorType ShouldMonitor { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }

        public override bool Enable => EnableAutomaticAdd;

        public ImportListStatus Status { get; set; }
        public ImportListType ListType { get; set; }
        public TimeSpan MinRefreshInterval { get; set; }
    }

    public enum ImportListMonitorType
    {
        None,
        SpecificEpisode,
        EntireSite
    }
}
