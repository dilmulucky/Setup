using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Setup.Models
{
    public class VersionModel
    {
        public int Id { get; set; }


        public string Version { get; set; }

        public string Describe { get; set; }

        public string MD5 { get; set; }

        public string Url { get; set; }

        public bool IsConstraint { get; set; }

        public bool IsFull { get; set; }
    }
}
