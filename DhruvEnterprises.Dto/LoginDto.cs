using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DhruvEnterprises.Dto
{
    public class LoginDto
    {
        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }

        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }

        [DisplayName("returnurl")]
        public string returnurl { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public int UserId { get; set; }

        public Guid ResetToken { get; set; }

        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordDto
    {
        public int UserId { get; set; }

        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

}
