using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoAn.Domain.Validations
{
    public class ValidateInput
    {
        public static bool IsValidEmail(string email)
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        public static bool IsValidPhoneNumber(string phonenumber)
        {
            string pattern = @"^(84|0[35789])[0-9]{8}";
           return Regex.IsMatch(phonenumber, pattern);
        }
    }
}
