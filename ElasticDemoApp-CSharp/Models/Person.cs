using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticDemoApp_CSharp.Models
{
    public class Person
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsManager { get; set; }
        public DateTime StartedOn { get; set; }
    }
}
