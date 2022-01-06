using meeting_console_app.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = meeting_console_app.Models.Type;

namespace meeting_console_app.CLIViews.MeetingViews
{
    public sealed class MeetingsListView : CLIView
    {
        public MeetingsListView(CLIContext context, IEnumerable<Meeting> meetings) : base(context)
        {
            _meetings = new(meetings);
            _commands = GenerateCommands();
        }

        public MeetingsListView(CLIContext context, IEnumerable<Meeting> meetings, string successMsg, string failureMsg) : base(context, successMsg, failureMsg)
        {
            _meetings = new(meetings);
            _commands = GenerateCommands();
        }

        protected override string ViewName => "Meetings list";

        protected override string ViewDescription => "Here you can see all the meetings and filter them by your choice";

        protected override List<Command> GenerateCommands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new ReturnToMainCommand());
            commands.Add(new QuitCommand());
            commands.Add(new ClearFiltersCommand());
            commands.Add(new FilterDescriptionCommand(_meetings));
            commands.Add(new FilterResponsiblePersonCommand(_meetings));
            commands.Add(new FilterCategoryCommand(_meetings));
            commands.Add(new FilterTypeCommand(_meetings));
            commands.Add(new FilterDatesCommand(_meetings));
            commands.Add(new FilterAttendeeNumber(_meetings));
            foreach (var meeting in _meetings)
            {
                commands.Add(new ViewMeetingCommand(meeting));
            }
            return commands;
        }

        private List<Meeting> _meetings;


        private class ViewMeetingCommand : Command
        {
            public ViewMeetingCommand(Meeting meeting)
            {
                _meeting = meeting;
            }

            public override string InvocationName => $"V{_meeting.Id}";

            public override string[] Arguments => Array.Empty<string>();

            public override string Description => $"View meeting \'{_meeting.Name}\' created by {_meeting.ResponsiblePerson} (Description: {_meeting.Description})";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                return new MeetingInfoView(context, _meeting);
            }

            private Meeting _meeting;
        }

        private class ReturnToMainCommand : Command
        {
            public override string InvocationName => "R";

            public override string[] Arguments => Array.Empty<string>();

            public override string Description => "Return to main menu";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                return new MainMeetingView(context);
            }
        }

        private class ClearFiltersCommand : Command
        {
            public override string InvocationName => "C";

            public override string[] Arguments => Array.Empty<string>();

            public override string Description => "Clear all the filters";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                IEnumerable<Meeting> meetings;
                context.DbInMemory.GetMeetings(out meetings);

                return new MeetingsListView(context, meetings);
            }
        }

        private class FilterDescriptionCommand : Command
        {
            public FilterDescriptionCommand(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FDesc";

            public override string[] Arguments => new string[] { "Description fragment" };

            public override string Description => "Filter meetings by description";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                string substring = string.Join(" ", args);
                _meetings = _meetings.Where(m => m.Description.Contains(substring)).ToList();
                return new MeetingsListView(context, _meetings, "Applied filter by description", "");
            }

            private List<Meeting> _meetings;
        }

        private class FilterResponsiblePersonCommand : Command
        {
            public FilterResponsiblePersonCommand(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FP";

            public override string[] Arguments => new string[] { "Responsible person's name" };

            public override string Description => "Filter meetings by meeting creator";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                string substring = string.Join(" ", args);
                _meetings = _meetings.Where(m => m.ResponsiblePerson == substring).ToList();
                return new MeetingsListView(context, _meetings, "Applied filter of filtering via creator's name", "");
            }

            private List<Meeting> _meetings;
        }

        private class FilterCategoryCommand : Command
        {
            public FilterCategoryCommand(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FC";

            public override string[] Arguments => new string[] { "Category name" };

            public override string Description
            {
                get
                {
                    string query = "Filter by Category (";
                    foreach (var value in Enum.GetValues(typeof(Category)))
                    {
                        query += $"{value.ToString()}, ";
                    }
                    query = query[0..^2];
                    query += "): ";
                    return query;
                }
            }

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length != 1)
                    return new MeetingsListView(context, _meetings, "", "Incorrect argument length");
                Category category;
                if (!Enum.TryParse<Category>(args[0], true, out category))
                    return new MeetingsListView(context, _meetings, "", "Category not found");

                _meetings = _meetings.Where(m => m.Category == category).ToList();

                return new MeetingsListView(context, _meetings, "Applied filter to categories", "");
            }

            private List<Meeting> _meetings;
        }

        private class FilterTypeCommand : Command
        {
            public FilterTypeCommand(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FT";

            public override string[] Arguments => new string[] { "Type name" };

            public override string Description
            {
                get
                {
                    string query = "Filter by Type (";
                    foreach (var value in Enum.GetValues(typeof(Type)))
                    {
                        query += $"{value.ToString()}, ";
                    }
                    query = query[0..^2];
                    query += "): ";
                    return query;
                }
            }

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length != 1)
                    return new MeetingsListView(context, _meetings, "", "Incorrect argument length");
                Type type;
                if (!Enum.TryParse<Type>(args[0], true, out type))
                    return new MeetingsListView(context, _meetings, "", "Category not found");

                _meetings = _meetings.Where(m => m.Type == type).ToList();

                return new MeetingsListView(context, _meetings, "Applied filter to categories", "");
            }

            private List<Meeting> _meetings;
        }

        private class FilterDatesCommand : Command
        {
            public FilterDatesCommand(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FDates";

            public override string[] Arguments => new string[] { $"Start Date ({Program.DayTimeFormat})", $"End Date ({Program.DayTimeFormat})" };

            public override string Description => "Filter all meetings that are in the date range";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length != 2)
                    return new MeetingsListView(context, _meetings, "", "Incorrect argument length");

                DateTime stDate, endDate;
                if (!DateTime.TryParseExact(args[0], Program.DayTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out stDate))
                    return new MeetingsListView(context, _meetings, "", "Incorrect date format for start date provided");
                if (!DateTime.TryParseExact(args[1], Program.DayTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    return new MeetingsListView(context, _meetings, "", "Incorrect date format for end date provided");
                if (stDate >= endDate)
                    return new MeetingsListView(context, _meetings, "", "Start date cannot be later than end date");

                _meetings = _meetings.Where(m => m.StartDate >= stDate && m.EndDate <= endDate).ToList();

                return new MeetingsListView(context, _meetings, "Applied filter with dates", "");
            }

            private List<Meeting> _meetings;
        }

        private class FilterAttendeeNumber : Command
        {
            public FilterAttendeeNumber(List<Meeting> meetings)
            {
                _meetings = new(meetings);
            }
            public override string InvocationName => "FN";

            public override string[] Arguments => new string[] { "Attendee number" };

            public override string Description => "Filter by number of minimal attendee amount";

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                if (args.Length != 1)
                    return new MeetingsListView(context, _meetings, "", "Incorrect argument length");

                int count;
                if (!int.TryParse(args[0], out count))
                    return new MeetingsListView(context, _meetings, "", "Provided argument is not a number");

                _meetings = _meetings.Where(m => m.Attendees.Count >= count).ToList();

                return new MeetingsListView(context, _meetings, "Applied filter with attendee count", "");
            }

            private List<Meeting> _meetings;
        }
    }
}
