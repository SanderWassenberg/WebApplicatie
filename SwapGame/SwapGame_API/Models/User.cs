using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace SwapGame_API.Models {
    public class User {
        [Key]        public int Id { get; set; }
        [Required, SG_Unique] public string Name { get; set; }

        // [Range] works like 0..2, the min and max value are INCLUSIVE.
        [Range(0,2)] public int ProfilePic { get; set; } // fixed number referring to one of a few default images.
        [Required]   public string HashedPassword { get; set; }
    }

    // https://stackoverflow.com/questions/56565612/how-to-validate-if-email-attribute-is-unique-in-an-asp-net-core-api
    public class SG_Unique : ValidationAttribute {
        protected override ValidationResult IsValid(object name, ValidationContext validationContext) {
            var _context = (SwapGame_DbContext) validationContext.GetService(typeof(SwapGame_DbContext));
            var user = _context.Users.SingleOrDefault(e => e.Name == name.ToString());

            if (user is not null) {
                return new ValidationResult($"Username ${name} is already in use.");
            }
            return ValidationResult.Success;
        }
    }
}
