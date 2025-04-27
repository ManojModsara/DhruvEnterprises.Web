using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class HomeDto
    {
        public string  AccountNo { get; set; }
        public string SCount { get; set; }
        public string SUCCESS { get; set; }
        public string PCount { get; set; }

        public string Proceess { get; set; }
        public string FCount { get; set; }

        public string Failed { get; set; }
        public string Eran { get; set; }
        public string Complaints { get; set; }
        public string TFComplaints { get; set; }
        public string TProfit { get; set; }

        public string Airtel { get; set; }
        public string Idea { get; set; }
        public string Jio { get; set; }
        public string Bsnl { get; set; }
        public string Voda { get; set; }
        public string News { get; set; }
    }

    public class Chart2
    {
        public dynamic Data { get; set; }
    }
}
