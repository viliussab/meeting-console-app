using meeting_console_app.Data;
using meeting_console_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meeting_console_app.Data
{
    internal interface IMeeting
    {
        public DbResponse CreateMeeting(Meeting meeting);
        public DbResponse DeleteMeeting(int id);
        public DbResponse AddAttendee(int meetingId, string person);
        public DbResponse RemoveAttendee(int meetingId, string person);
        public DbResponse GetMeetings(out IEnumerable<Meeting> meetings);
    }
}
