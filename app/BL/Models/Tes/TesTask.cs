﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*
 * Task Execution Service
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * OpenAPI spec version: 0.3.0
 *
 * Generated by: https://openapi-generator.tech
 */


using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using BL.Utilities;

namespace BL.Models.Tes
{
    /// <summary>
    /// Task describes an instance of a task.
    /// </summary>
    [DataContract]
    public partial class TesTask : IEquatable<TesTask>
    {
        public TesTask()
            => NewtonsoftJsonSafeInit.SetDefaultSettings();

        /// <summary>
        /// Task identifier assigned by the server.
        /// </summary>
        /// <value>Task identifier assigned by the server.</value>
        [DataMember(Name = "id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [DataMember(Name = "state")]
        public TesState State { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description")]
        public string? Description { get; set; }

        /// <summary>
        /// Input files. Inputs will be downloaded and mounted into the executor container.
        /// </summary>
        /// <value>Input files. Inputs will be downloaded and mounted into the executor container.</value>
        [DataMember(Name = "inputs")]
        public List<TesInput>? Inputs { get; set; }

        /// <summary>
        /// Output files. Outputs will be uploaded from the executor container to long-term storage.
        /// </summary>
        /// <value>Output files. Outputs will be uploaded from the executor container to long-term storage.</value>
        [DataMember(Name = "outputs")]
        public List<TesOutput>? Outputs { get; set; }

        /// <summary>
        /// Gets or Sets Resources
        /// </summary>
        [DataMember(Name = "resources")]
        public TesResources? Resources { get; set; }

        /// <summary>
        /// A list of executors to be run, sequentially. Execution stops on the first error.
        /// </summary>
        /// <value>A list of executors to be run, sequentially. Execution stops on the first error.</value>
        [DataMember(Name = "executors")]
        public List<TesExecutor> Executors { get; set; }

        /// <summary>
        /// Volumes are directories which may be used to share data between Executors. Volumes are initialized as empty directories by the system when the task starts and are mounted at the same path in each Executor.  For example, given a volume defined at \&quot;/vol/A\&quot;, executor 1 may write a file to \&quot;/vol/A/exec1.out.txt\&quot;, then executor 2 may read from that file.  (Essentially, this translates to a &#x60;docker run -v&#x60; flag where the container path is the same for each executor).
        /// </summary>
        /// <value>Volumes are directories which may be used to share data between Executors. Volumes are initialized as empty directories by the system when the task starts and are mounted at the same path in each Executor.  For example, given a volume defined at \&quot;/vol/A\&quot;, executor 1 may write a file to \&quot;/vol/A/exec1.out.txt\&quot;, then executor 2 may read from that file.  (Essentially, this translates to a &#x60;docker run -v&#x60; flag where the container path is the same for each executor).</value>
        [DataMember(Name = "volumes")]
        public List<string>? Volumes { get; set; }

        /// <summary>
        /// A key-value map of arbitrary tags.
        /// </summary>
        /// <value>A key-value map of arbitrary tags.</value>
        [DataMember(Name = "tags")]
        public Dictionary<string, string>? Tags { get; set; }

        /// <summary>
        /// Task logging information. Normally, this will contain only one entry, but in the case where a task fails and is retried, an entry will be appended to this list.
        /// </summary>
        /// <value>Task logging information. Normally, this will contain only one entry, but in the case where a task fails and is retried, an entry will be appended to this list.</value>
        [DataMember(Name = "logs")]
        public List<TesTaskLog>? Logs { get; set; }

        /// <summary>
        /// Date + time the task was created, in RFC 3339 format. This is set by the system, not the client.
        /// </summary>
        /// <value>Date + time the task was created, in RFC 3339 format. This is set by the system, not the client.</value>
        [DataMember(Name = "creation_time")]
        public DateTimeOffset? CreationTime { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
            => new StringBuilder()
                .Append("class TesTask {\n")
                .Append("  Id: ").Append(Id).Append('\n')
                .Append("  State: ").Append(State).Append('\n')
                .Append("  Name: ").Append(Name).Append('\n')
                .Append("  Description: ").Append(Description).Append('\n')
                .Append("  Inputs: ").Append(Inputs).Append('\n')
                .Append("  Outputs: ").Append(Outputs).Append('\n')
                .Append("  Resources: ").Append(Resources).Append('\n')
                .Append("  Executors: ").Append(Executors).Append('\n')
                .Append("  Volumes: ").Append(Volumes).Append('\n')
                .Append("  Tags: ").Append(Tags).Append('\n')
                .Append("  Logs: ").Append(Logs).Append('\n')
                .Append("  CreationTime: ").Append(CreationTime).Append('\n')
                .Append("}\n")
                .ToString();

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
            => obj switch
            {
                var x when x is null => false,
                var x when ReferenceEquals(this, x) => true,
                _ => obj.GetType() == GetType() && Equals((TesTask)obj),
            };

        /// <summary>
        /// Returns true if TesTask instances are equal
        /// </summary>
        /// <param name="other">Instance of TesTask to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TesTask other)
            => other switch
            {
                var x when x is null => false,
                var x when ReferenceEquals(this, x) => true,
                _ =>
                (
                    Id == other.Id ||
                    Id is not null &&
                    Id.Equals(other.Id)
                ) &&
                (
                    State == other.State ||
                    State.Equals(other.State)
                ) &&
                (
                    Name == other.Name ||
                    Name is not null &&
                    Name.Equals(other.Name)
                ) &&
                (
                    Description == other.Description ||
                    Description is not null &&
                    Description.Equals(other.Description)
                ) &&
                (
                    Inputs == other.Inputs ||
                    Inputs is not null &&
                    Inputs.SequenceEqual(other.Inputs)
                ) &&
                (
                    Outputs == other.Outputs ||
                    Outputs is not null &&
                    Outputs.SequenceEqual(other.Outputs)
                ) &&
                (
                    Resources == other.Resources ||
                    Resources is not null &&
                    Resources.Equals(other.Resources)
                ) &&
                (
                    Executors == other.Executors ||
                    Executors is not null &&
                    Executors.SequenceEqual(other.Executors)
                ) &&
                (
                    Volumes == other.Volumes ||
                    Volumes is not null &&
                    Volumes.SequenceEqual(other.Volumes)
                ) &&
                (
                    Tags == other.Tags ||
                    Tags is not null &&
                    Tags.SequenceEqual(other.Tags)
                ) &&
                (
                    Logs == other.Logs ||
                    Logs is not null &&
                    Logs.SequenceEqual(other.Logs)
                ) &&
                (
                    CreationTime == other.CreationTime ||
                    CreationTime is not null &&
                    CreationTime.Equals(other.CreationTime)
                ),
            };

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (Id is not null)
                {
                    hashCode = hashCode * 59 + Id.GetHashCode();
                }

                hashCode = hashCode * 59 + State.GetHashCode();
                if (Name is not null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                if (Description is not null)
                {
                    hashCode = hashCode * 59 + Description.GetHashCode();
                }

                if (Inputs is not null)
                {
                    hashCode = hashCode * 59 + Inputs.GetHashCode();
                }

                if (Outputs is not null)
                {
                    hashCode = hashCode * 59 + Outputs.GetHashCode();
                }

                if (Resources is not null)
                {
                    hashCode = hashCode * 59 + Resources.GetHashCode();
                }

                if (Executors is not null)
                {
                    hashCode = hashCode * 59 + Executors.GetHashCode();
                }

                if (Volumes is not null)
                {
                    hashCode = hashCode * 59 + Volumes.GetHashCode();
                }

                if (Tags is not null)
                {
                    hashCode = hashCode * 59 + Tags.GetHashCode();
                }

                if (Logs is not null)
                {
                    hashCode = hashCode * 59 + Logs.GetHashCode();
                }

                if (CreationTime is not null)
                {
                    hashCode = hashCode * 59 + CreationTime.GetHashCode();
                }

                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(TesTask left, TesTask right)
            => Equals(left, right);

        public static bool operator !=(TesTask left, TesTask right)
            => !Equals(left, right);

#pragma warning restore 1591
        #endregion Operators
    }
}
