using FluentValidation;
using DhruvEnterprises.Dto;

namespace DhruvEnterprises.Validation
{
 
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {

        public LoginDtoValidator()
        {
             
            RuleFor(l => l.Email).NotEmpty().WithMessage("*required");
            RuleFor(l => l.Password).NotEmpty().WithMessage("*required");
            

        }

    }
}
