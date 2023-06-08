using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    public class ExceptionViewModel
    {
        public List<Event> EventData { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ExceptionViewModel()
        {

        }

        public ExceptionViewModel(List<Event> eventData)
        {
            this.EventData = eventData;
        }
    }
}
