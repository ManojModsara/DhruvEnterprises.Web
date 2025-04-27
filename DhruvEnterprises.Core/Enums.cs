using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Core
{
    public static class Enums
    {
        public enum AppraisalMarking
        {
            Productivity = 1,
            Communication = 2,
            LeaderShip = 3,
            PersonalDevelopment = 4,
            RelationShips = 5,
            Management = 6
        }

        public enum UserRoles
        {
            DV = 1,
            SD = 2,
            TL = 3,
            PM = 4,
            HR = 5,
            QA = 6,
            PMO = 7,
            DG = 8,
            BA = 9,
            TLS = 10,
            SEO = 11,
            OP = 12,
            DEO = 13,
            UKPM = 14
        }
        // # Added by Pooja #
        public enum ModalSize
        {
            Small,
            Large,
            Medium,
            XLarge
        }
        
        public enum MessageType
        {
            Warning,
            Success,
            Danger,
            Info
        }
       
        // End
        public static bool CompareDate(this DateTime? dateTime, DateTime? otherDate)
        {
            if (dateTime.HasValue && otherDate.HasValue)
            {
                return dateTime.Value.Day == otherDate.Value.Day
                       && dateTime.Value.Month == otherDate.Value.Month
                       && dateTime.Value.Year == otherDate.Value.Year;
            }

            return false;

        }
        
    }
}
