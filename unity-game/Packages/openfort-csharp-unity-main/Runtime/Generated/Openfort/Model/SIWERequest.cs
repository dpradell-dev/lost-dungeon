/*
 * Openfort API
 *
 * Complete Openfort API references and guides can be found at: https://openfort.xyz/docs
 *
 * The version of the OpenAPI document: 1.0.0
 * Contact: founders@openfort.xyz
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpenAPIDateConverter = Openfort.Client.OpenAPIDateConverter;

namespace Openfort.Model
{
    /// <summary>
    /// SIWERequest
    /// </summary>
    [DataContract(Name = "SIWERequest")]
    public partial class SIWERequest : IEquatable<SIWERequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SIWERequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SIWERequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SIWERequest" /> class.
        /// </summary>
        /// <param name="address">The address of the user. (required).</param>
        public SIWERequest(string address = default(string))
        {
            // to ensure "address" is required (not null)
            if (address == null)
            {
                throw new ArgumentNullException("address is a required property for SIWERequest and cannot be null");
            }
            this.Address = address;
        }

        /// <summary>
        /// The address of the user.
        /// </summary>
        /// <value>The address of the user.</value>
        /// <example>&quot;0x8C5cedA46A26214A52A9D7BF036Ad2F6255BdBEa&quot;</example>
        [DataMember(Name = "address", IsRequired = true, EmitDefaultValue = true)]
        public string Address { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SIWERequest {\n");
            sb.Append("  Address: ").Append(Address).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as SIWERequest);
        }

        /// <summary>
        /// Returns true if SIWERequest instances are equal
        /// </summary>
        /// <param name="input">Instance of SIWERequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SIWERequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Address == input.Address ||
                    (this.Address != null &&
                    this.Address.Equals(input.Address))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Address != null)
                {
                    hashCode = (hashCode * 59) + this.Address.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
