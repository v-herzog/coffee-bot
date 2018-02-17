using System;
using System.Collections.Generic;

namespace CoffeeBot.Models
{
    [Serializable]
    public class AnalyzeResult
    {
        public string requestId { get; set; }
        public List<Tag> tags { get; set; }
    }
}