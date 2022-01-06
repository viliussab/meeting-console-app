using meeting_console_app.Data;
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
    public sealed class MainMeetingView : CLIView
    {
        public MainMeetingView(CLIContext context) : base(context)
        {
            _commands = GenerateCommands();
        }

        public MainMeetingView(CLIContext context, string successMsg, string failureMsg) : base(context, successMsg, failureMsg)
        {
            _commands = GenerateCommands();
        }

        protected override string ViewName => "Welcome to the meeting manager program!";

        protected override string ViewDescription => "You can navigate by typing commands \n" +
            "Available commands are indicated below in brackets\n" +
            "For example if a command is written as [Q], you need to type Q and press <Enter>";

        protected override List<Command> GenerateCommands()
        {
            List<Command> commands = new();
            commands.Add(new CreateMeetingCommand());
            commands.Add(new ViewMeetingListCommand());
            commands.Add(new QuitCommand());
            return commands;
        }

        private class ViewMeetingListCommand : Command
        {
            public override string InvocationName => "V";
            public override string Description => "View all the meetings";
            public override string[] Arguments => Array.Empty<string>();

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                IEnumerable<Meeting> meetings;
                context.DbInMemory.GetMeetings(out meetings);

                return new MeetingsListView(context, meetings);
            }
        }

        private class CreateMeetingCommand : Command
        {

            public override string InvocationName => "C";
            public override string Description => "Create new meeting";
            public override string[] Arguments => Array.Empty<string>();

            public override CLIView ExecuteCommand(CLIContext context, string[] args)
            {
                Meeting meeting = new Meeting();
                Console.Clear();

                Console.Write("Authenticate yourself - what is your name?: ");
                meeting.ResponsiblePerson = Console.ReadLine();

                Console.Write("Enter meeting name: ");
                meeting.Name = Console.ReadLine();

                Console.Write("Write the description of the meeting: ");
                meeting.Description = Console.ReadLine();

                // Read Category
                Category? category = null;
                while (category == null)
                {
                    string query = "Which category it belongs to? (";
                    foreach (var value in Enum.GetValues(typeof(Category)))
                    {
                        query += $"{value.ToString()}, ";
                    }
                    query = query[0..^2];
                    query += "): ";
                    Console.Write(query);
                    var output = Console.ReadLine();
                    Category cat;
                    if (Enum.TryParse<Category>(output, true, out cat))
                        category = cat;
                    else
                        Console.WriteLine("Category typed incorrectly, please try again");
                }
                meeting.Category = (Category)category;

                // Read Type
                Type? type = null;
                while (type == null)
                {
                    string query = "Which type it belongs to? (";
                    foreach (var value in Enum.GetValues(typeof(Type)))
                    {
                        query += $"{value.ToString()}, ";
                    }
                    query = query[0..^2];
                    query += "): ";
                    Console.Write(query);
                    var output = Console.ReadLine();
                    Type typ;           
                    if (Enum.TryParse<Type>(output, true, out typ))
                        type = typ;
                    else
                        Console.WriteLine("Type written incorrectly, please try again");
                }
                meeting.Type = (Type)type;

                // Read start date
                while (true)
                {
                    DateTime stDate;
                    Console.Write("Write when the meeting starts (date format: \'yyyy-MM-dd HH:mm\'): ");
                    var output = Console.ReadLine();
                    if (!DateTime.TryParseExact(output, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out stDate))
                    {
                        Console.WriteLine("Incorrect date format");
                        continue;
                    }
                    if (stDate <= DateTime.Now)
                    {
                        Console.WriteLine("Date provided has already passed!");
                        continue;
                    }

                    meeting.StartDate = stDate;
                    break;
                }

                // Read end date
                while (true)
                {
                    DateTime endDate;
                    Console.Write("Write when the meeting ends (date format: \'yyyy-MM-dd HH:mm\'): ");
                    var output = Console.ReadLine();
                    if (!DateTime.TryParseExact(output, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    {
                        Console.WriteLine("Incorrect date format");
                        continue;
                    }
                    if (endDate <= DateTime.Now)
                    {
                        Console.WriteLine("Date provided has already passed!");
                        continue;
                    }
                    if (endDate < meeting.StartDate)
                    {
                        Console.WriteLine("Meeting can not end earlier than it starts!");
                        continue;
                    }

                    meeting.EndDate = endDate;
                    break;
                }

                meeting.Id = context.DbInMemory.IdForNewElement;
                meeting.Attendees = new();

                DbResponse res = context.DbInMemory.CreateMeeting(meeting);

                if (res.isSuccess)
                {
                    return new MainMeetingView(context, res.sucessMsg, "");
                }
                else
                {
                    return new MainMeetingView(context, "", res.errorMsg);
                }
                
            }

        }
    }
}
