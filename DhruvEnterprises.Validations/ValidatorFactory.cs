using FluentValidation;
using System;
using System.Collections.Generic;
using DhruvEnterprises.Dto;

namespace DhruvEnterprises.Validation
{
    public class ValidatorFactory : ValidatorFactoryBase
    {
        private static Dictionary<Type, IValidator> _validators = new Dictionary<Type, IValidator>();

        static ValidatorFactory()
        {
            _validators.Add(typeof(IValidator<LoginDto>), new LoginDtoValidator());

            //_validators.Add(typeof(IValidator<ForgotPasswordDto>), new ForgotPasswordDtoValidator());
            //_validators.Add(typeof(IValidator<ForgotPasswordDto>), new ForgotPasswordDtoValidator());
            //_validators.Add(typeof(IValidator<ChangePasswordDto>), new ChangePasswordDtoValidator());
            //_validators.Add(typeof(IValidator<LoginDto>), new FacilityDtoValidation());
            //_validators.Add(typeof(IValidator<UserSessionDto>), new DestinationDtoValidator());
        }
        /// <summary>
        /// Creates an instance of a validator with the given type.
        /// </summary>
        /// <param name="validatorType">Type of the validator.</param>
        /// <returns>The newly created validator</returns>
        public override IValidator CreateInstance(Type validatorType)
        {
            IValidator validator;
            if (_validators.TryGetValue(validatorType, out validator))
                return validator;
            return validator;
        }
    }
}