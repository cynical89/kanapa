using Newtonsoft.Json;

namespace Kanapa
{
  public class DatabaseMetadata
  {
    [JsonProperty("committed_update_seq")]
    public long CommitedUpdateNumber { get; set; }
    [JsonProperty("compact_running ")]
    public bool IsCompactionRunning { get; set; }
    [JsonProperty("db_name ")]
    public string Name { get; set; }
    [JsonProperty("disk_format_version")]
    public int DiskFormatVersion { get; set; }
    [JsonProperty("data_size")]
    public long DataSize { get; set; }
    [JsonProperty("disk_size")]
    public long DiskSize { get; set; }
    [JsonProperty("doc_count")]
    public long DocumentCount { get; set; }
    [JsonProperty("doc_del_count")]
    public long DeletedDocuments { get; set; }
    [JsonProperty("instance_start_time")]
    public string InstanceStartTime { get; set; }
    [JsonProperty("purge_seq")]
    public long PurgeOperationsCount { get; set; }
    [JsonProperty("update_seq")]
    public long CurrentUpdatesCount { get; set; }
  }
}