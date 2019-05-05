using System;
using Newtonsoft.Json;

namespace AltinnCore.Common.Models
{
    /// <summary>
    /// Model for form data
    /// </summary>
    [Serializable]
    public class Data
    {
        /// <summary>
        /// users filename
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// users filename
        /// </summary>
        [JsonProperty(PropertyName = "formId")]
        public string FormId { get; set; }

        /// <summary>
        /// users filename
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// contentType of file in blob
        /// </summary>
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// path to blob storage
        /// </summary>
        [JsonProperty(PropertyName = "storageUrl")]
        public string StorageUrl { get; set; }

        /// <summary>
        /// Size of file in bytes
        /// </summary>
        [JsonProperty(PropertyName = "fileSize")]
        public int FileSize { get; set; }

        /// <summary>
        /// Signature
        /// </summary>
        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        /// <summary>
        /// Size of file in bytes
        /// </summary>
        [JsonProperty(PropertyName = "isLocked")]
        public bool IsLocked { get; set; }

        /// <summary>
        /// create date and time for the instance
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// reportee id of the user who created the instance
        /// </summary>
        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// last changed date time for the instance
        /// </summary>
        [JsonProperty(PropertyName = "lastChangedDateTime")]
        public DateTime LastChangedDateTime { get; set; }

        /// <summary>
        /// reportee id of the user who last changed the instance
        /// </summary>
        [JsonProperty(PropertyName = "lastChangedBy")]
        public string LastChangedBy { get; set; }

        /// <summary>
        /// data type of the uploaded data
        /// </summary>
        [JsonProperty(PropertyName = "dataType")]
        public string DataType { get; set; }
    }
}
