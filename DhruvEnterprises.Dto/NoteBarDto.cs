using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class NoteBarDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string NotificationMsg { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public bool IsFire { get; set; }


    }
}
