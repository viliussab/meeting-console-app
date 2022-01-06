using meeting_console_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meeting_console_app.Data
{
    public class Database
    {
        public Database()
        {
            Meeting = new List<Meeting>();
        }
        public Database(DbInMemory dbInMemory)
        {
            IEnumerable<Meeting> meetings;
            dbInMemory.GetMeetings(out meetings);
            Meeting = new List<Meeting>(meetings);
        }
        public List<Meeting> Meeting { get; set; }
    }
}
