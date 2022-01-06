using meeting_console_app.Data;
using meeting_console_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = meeting_console_app.Models.Type;

namespace meeting_console_app.CLIViews.MeetingViews
{
    public sealed class MeetingInfoView : CLIView
    {
        public MeetingInfoView(CLIContext context, Meeting meeting) : base(context)
        {
            _meeting = meeting;
            _commands = GenerateCommands();
        }

        public MeetingInfoView(CLIContext context, Meeting meeting, string successMsg, string failureMsg) : base(context, successMsg, failureMsg)
        {
            _meeting = meeting;
            _commands = GenerateCommands();
        }

        protected override string ViewName => $"Meeting {_meeting.Name}";
        protected override string ViewDescription {
            get
            {
                string ans = "";
                ans += "Description: " + _meeting.Description + "\n";
                ans += "Created by: " + _meeting.ResponsiblePerson + "\n";
                ans += "Category: " + Enum.GetName(typeof(Category), _meeting.Category) + "\n";
                ans += "Type: " + Enum.GetName(typeof(Type), _meeting.Type) + "\n";
                ans += "Starts from: " + _meeting.StartDate.ToString("yyyy-MM-dd HH:mm") + "\n";
                ans += "Ends at: " + _meeting.EndDate.ToString("yyyy-MM-dd HH:mm") + "\n";
                ans += "Attendees: ";
                foreach (var attendee in _meeting.Attendees)
                {
                    ans += $"{attendee}, ";
                }
                // Remove last ", "
                ans = ans[0..^2];
                return ans;
            }
        }

        protected override List<Command> GenerateCommands()
        {
            List<Command> commands = new List<Command>();

            commands.Add(new RemoveMeetingCommand(_meeting));
            commands.Add(new AddAttendeeCommand(_meeting));
            commands.Add(new RemoveAttendeeCommand(_meeting));
            commands.Add(new ReturnToListCommand());
            commands.Add(new QuitCommand());

            return commands;
        }

        private Meeting _meeting;

        private class RemoveMeetingCommand : Command
        {
            public RemoveMeetingCommand(Meeting meeting)
            {
                _meeting = meeting;
            }

            public override string InvocationName => "D";

            public override string[] Arguments => Array.Empty<string>();

            public override string Description => "Delete a meeting entirely";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                Console.WriteLine("Are you sure you want to delete the meeting [y/n]");
                string c = Console.ReadLine();
                if (c != "y")
                {
                    return new MeetingInfoView(context, _meeting, "", "Delete operation aborted");
                }

                DbResponse ans = context.DbInMemory.DeleteMeeting(_meeting.Id);

                IEnumerable<Meeting> meetings;
                context.DbInMemory.GetMeetings(out meetings);

                if (ans.isSuccess)
                    return new MeetingsListView(context, meetings, ans.sucessMsg, "");
                else
                    return new MeetingsListView(context, meetings, "", ans.errorMsg);
            }
            private Meeting _meeting;
        }

        private class AddAttendeeCommand : Command
        {

            public AddAttendeeCommand(Meeting meeting)
            {
                _meeting = meeting;
            }
            public override string InvocationName => "A";

            public override string[] Arguments => new string[] { "Attendee name" };

            public override string Description => "Add a new attendee to this meeting";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length == 0)
                    return new MeetingInfoView(context, _meeting, "", "No argument for person found");
                string name = args[0];
                if (args.Length > 1)
                    name = string.Join(" ", args);

                DbResponse res = context.DbInMemory.AddAttendee(_meeting.Id, name);

                if (res.isSuccess)
                {
                    IEnumerable<Meeting> meetings;
                    context.DbInMemory.GetMeetings(out meetings);
                    IEnumerable<Meeting> overlappingMeetings = GetOverlappingMeetings(meetings, _meeting, name);
                    return new MeetingInfoView(context, _meeting, res.sucessMsg, OverlappingMeetingsText(overlappingMeetings, name));
                }
                else
                {
                    return new MeetingInfoView(context, _meeting, "", res.errorMsg);
                }
            }

            private static string OverlappingMeetingsText(IEnumerable<Meeting> overlappingMeetings, string attendeeName)
            {
                if (overlappingMeetings.Any())
                {
                    var meetings = overlappingMeetings.Select(m => m.Name).ToArray();
                    if (meetings == null)
                        return "";

                    string message = "";
                    if (meetings.Count() == 1)   
                        message = $"{attendeeName} will have overlap with meeting {meetings[0]}";
                    else
                    {
                        message = $"{attendeeName} will have overlap with meetings {String.Join(", ", meetings)}";
                        message = message[0..^2];
                    }

                    return message;
                }
                else
                {
                    return "";
                }
            }

            private static IEnumerable<Meeting> GetOverlappingMeetings(IEnumerable<Meeting> allMeetings, Meeting analyzedMeeting, string attendee)
            {
                List<Meeting> overlappers = new List<Meeting>();
                foreach (Meeting other in allMeetings)
                {
                    if (other.Equals(analyzedMeeting))
                        continue;

                    if (!other.Attendees.Contains(attendee))
                        continue;

                    if ((analyzedMeeting.StartDate >= other.StartDate && analyzedMeeting.StartDate <= other.EndDate) 
                        ||
                        (analyzedMeeting.EndDate >= other.StartDate && analyzedMeeting.EndDate <= other.EndDate))
                        overlappers.Add(other);
                }
                return overlappers;
            }

            private Meeting _meeting;
        }

        private class RemoveAttendeeCommand : Command
        {
            public RemoveAttendeeCommand(Meeting meeting)
            {
                _meeting = meeting;
            }

            public override string InvocationName => "R";
       
            public override string[] Arguments => new string[] { "Attendee name" };

            public override string Description => "Remove an attendee from this meeting";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length == 0)
                    return new MeetingInfoView(context, _meeting, "", "No argument for person found");
                string name = args[0];
                if (args.Length > 1)
                    name = string.Join(" ", args);

                Console.WriteLine(name);
                DbResponse res = context.DbInMemory.RemoveAttendee(_meeting.Id, name);
                return new MeetingInfoView(context, _meeting, res.sucessMsg, res.errorMsg);
            }

            private Meeting _meeting;
        }

        private class ReturnToListCommand : Command
        {
            public override string InvocationName => "r";

            public override string[] Arguments => Array.Empty<string>();

            public override string Description => "Return to the list view";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                IEnumerable<Meeting> meetings;
                context.DbInMemory.GetMeetings(out meetings);

                return new MeetingsListView(context, meetings);
            }
        }
    }
}
