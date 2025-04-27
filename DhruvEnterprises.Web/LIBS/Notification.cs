using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static DhruvEnterprises.Core.Enums;

namespace DhruvEnterprises.Web.LIBS
{
    public class Notification
    {
        public string Heading { get; set; }
        public string Message { get; set; }
        public MessageType Type { get; set; }
        public string Icon
        {
            get
            {
                switch (this.Type)
                {
                    case MessageType.Warning:
                        return "icon-warning-sign";
                    case MessageType.Success:
                        return "icon-check-sign";
                    case MessageType.Danger:
                        return "icon-remove-sign";
                    case MessageType.Info:
                        return "icon-info-sign";
                    default:
                        return "icon-info-sign";
                }
            }
        }
    }
}