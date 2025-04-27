using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API.Models
{
    public class StatsCheckDto
    {
        public StatsCheckDto()
        {

        }
        public string merchantLoginId { get; set; }
        public string merchantPassword { get; set; }
        public string superMerchantId { get; set; }
        public string merchantTranId { get; set; }
        public string hash { get; set; }

    }

    public class AddSensderDto
    {

        [Required]
        public string Name { get; set; }
        [Required]
        public string MobileNo { get; set; }
        public string address { get; set; }
        //[Required]
        //public string Otp { get; set; }
        [Required]
        public string OpId { get; set; }
        public string TokenId { get; set; }
        [Required]
        public string dateOfBirth { get; set; }

    }
    public class FetchSenderDto
    {
        [Required]
        public string MobileNo { get; set; }
        [Required]
        public string OpId { get; set; }
        public string TokenId { get; set; }
       
    }
    public class Recipients
    {
        public string RecipientId { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string MobileNumber { get; set; }
        public string Ifsc { get; set; }
        public string RecipientName { get; set; }
     
    }
    public class AddBeneDto
    {
        public string RecipientId { get; set; }
        [Required]
        public string BankAccountNumber { get; set; }
        [Required]
        public string BankName { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Ifsc { get; set; }
        [Required]
        public string RecipientName { get; set; }
        [Required]
        public string customerId { get; set; }
        public string OpId { get; set; }
        public string TokenId { get; set; }
    }
    public class DeleteRecipientDto
    {
        [Required]
        public string MobileNo { get; set; }
        [Required]
        public string OpId { get; set; }
        public string TokenId { get; set; }
        [Required]
        public string recipientId { get; set; }

    }
    public class FetchBankListDto
    {
      
        [Required]
        public string OpId { get; set; }
        public string TokenId { get; set; }
     
    }
    public class BankListDto
    {
        public string BankCode { get; set; }
        public string accVerAvailabe { get; set; }
        public string BankName { get; set; }
        public string Ifsc { get; set; }
        public string ifscStatus { get; set; }
        public string channelsSupported { get; set; }

    }
}