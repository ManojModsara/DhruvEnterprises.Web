using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace DhruvEnterprises.Models.Secuirity
{
    public class CustomPrincipal : IPrincipal
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public int RoleId { get; set; }
        public bool IsSuperAdmin { get; set; }
        public byte[] Roles { get; set; }
        public bool IsAdminRole { get; set; }

        [JsonIgnore]
        public IIdentity Identity { get; private set; }

        public CustomPrincipal() { }

        public CustomPrincipal(string userName, params byte[] roleTypes)
        {
            this.Identity = new GenericIdentity(userName);
            this.Username = userName;
            this.Roles = roleTypes;
            this.RoleId = roleTypes.FirstOrDefault();
        
        }

        public CustomPrincipal(dynamic user, params byte[] roleTypes)
        {
            this.UserID = user.Id;
            this.Identity = new GenericIdentity(user.Username);
            this.Username = user.Username;
            this.RoleId = user.RoleId;
            this.Roles = roleTypes;
            this.IsAdminRole = user.IsAdminRole ?? false;
        }

        public bool IsInRole(Object roleType)
        {
            return Roles.Contains((byte)roleType);
        }

        public bool IsInRole(params Object[] roleTypes)
        {
            return roleTypes.Any(r => Roles.Contains((byte)r));
        }

        public bool IsInRole(string role)
        {
            //Check with enum
            //Object roleType;
            //if (Enum.TryParse(role, out roleType)) { return IsInRole(roleType); }
            return false;
        }
    }
}