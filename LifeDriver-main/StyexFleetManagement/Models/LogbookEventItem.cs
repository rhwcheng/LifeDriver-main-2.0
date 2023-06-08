using System;

namespace StyexFleetManagement.Models
{
    public class LogbookEventItem
    {

        public LogbookEventItem(string v)
        {
            Date = DateTime.Now;
            Description = v;
        }

        public DateTime Date { get; set; }
        public string DateString => Date.ToString("hh:mm:ss - dd MMM yyyy");
        public string Description { get; set; }
    }
}
