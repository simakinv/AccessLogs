using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AccessLogs.Service.Models
{
    public class GetTopClientsRequest: IValidatableObject
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int TopN { get; set; }

        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (To < From)
            {
                yield return new ValidationResult(
                    errorMessage: "To date must be greater than From date",
                    memberNames: new[] { "To" }
                );
            }
        }
    }
}
