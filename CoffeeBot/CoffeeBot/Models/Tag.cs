using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoffeeBot.Models
{
    [Serializable]
    public class Tag
    {
        public double confidence { get; set; }
        public string name { get; set; }
    }
}