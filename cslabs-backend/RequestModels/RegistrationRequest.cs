using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSLabsBackend.Models;
using CSLabsBackend.Util;
using Microsoft.EntityFrameworkCore;

namespace CSLabsBackend.RequestModels
{
    public class RegistrationRequest
    {
 
        [Required]
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [RegularExpression(@"^.+@ius\.edu$", ErrorMessage = "The email must end with @ius.edu")]
        [EitherRequired(nameof(PersonalEmail))]
        public string SchoolEmail { get; set; }
       
        [RegularExpression(@"^.+@.+\..+$", ErrorMessage = "Personal email must be a valid email address")]
        [EitherRequired(nameof(SchoolEmail))]
        public string PersonalEmail { get; set; }
        
        public int? GraduationYear { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
        
        public GenericErrorResponse Validate(DefaultContext dbContext)
        {
            if (SchoolEmail != null && dbContext.Users.Count(u => u.SchoolEmail == SchoolEmail) != 0)
            {
                return new GenericErrorResponse { Message = "The specified school email is already in use"};
            }
            if (PersonalEmail != null && dbContext.Users.Count(u => u.PersonalEmail == PersonalEmail) != 0)
            {
                return new GenericErrorResponse { Message = "The specified personal email is already in use"};
            }

            return null;
        }

        public bool IsValid(DefaultContext dbContext)
        {
            return Validate(dbContext) == null;
        }

        public string GetEmail()
        {
            return this.SchoolEmail ?? this.PersonalEmail;
        }
    }
}