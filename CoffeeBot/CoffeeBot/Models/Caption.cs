using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoffeeBot.Models
{
    [Serializable]
    public class Caption
    {
        public double confidence { get; set; }
        public string text { get; set; }
    }
}