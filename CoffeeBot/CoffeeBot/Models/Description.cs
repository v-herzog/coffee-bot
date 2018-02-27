using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoffeeBot.Models
{
    [Serializable]
    public class Description
    {
        public List<Caption> captions { get; set; }
        public List<string> tags { get; set; }
    }
}