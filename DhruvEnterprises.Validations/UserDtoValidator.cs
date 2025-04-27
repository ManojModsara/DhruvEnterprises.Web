using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using DhruvEnterprises.Dto;
namespace DhruvEnterprises.Validation
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        
        public UserDtoValidator()
        {
           
            RuleFor(l => l.Name).NotEmpty().WithMessage("*required");
            RuleFor(l => l.Username).NotEmpty().WithMessage("*required");
            RuleFor(l => l.EmailId).NotEmpty().WithMessage("*required");
          
            RuleFor(l => l.ContactNo).NotEmpty().WithMessage("*required");
                   
        }
        
    }
}
