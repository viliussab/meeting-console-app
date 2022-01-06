using meeting_console_app.Data;
using meeting_console_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Type = meeting_console_app.Models.Type;

namespace meeting_tests.MeetingTests
{
    public class MeetingAttendeeAddTests
    {
        private readonly Database db;
        private readonly DbInMemory dbInMemory;

        public MeetingAttendeeAddTests()
        {
            db = new Database();
            db.Meeting.Add(
                new Meeting(0,
                "Meeting",
                "Person1",
                "No description",
                Category.CodeMonkey,
                Type.InPerson, DateTime.Now,
                DateTime.Now + new TimeSpan(2, 0, 0),
                new()));

            db.Meeting[0].Attendees.Add("Attendee1");
            db.Meeting[0].Attendees.Add("Attendee2");

            dbInMemory = new DbInMemory(db);
        }

        [Fact]
        public void WhenAddingCorrectAttendee_ThenReturnShouldBeTrue()
        {
            string validPerson = "Attendee3";
            int validId = 0;

            var response = dbInMemory.AddAttendee(validId, validPerson);
            Assert.True(response.isSuccess);
        }

        [Fact]
        public void WhenAddingCorrectAttendee_ThenItShouldBeAddedToTheAttendeeList()
        {
            string validPerson = "Attendee3";
            int validId = 0;

            var response = dbInMemory.AddAttendee(validId, validPerson);
            Database updatedDatabase = new Database(dbInMemory);

            Assert.True(updatedDatabase.Meeting[0].Attendees.Exists(p => p == validPerson));
        }

        [Fact]
        public void WhenAddingAttendeeWithIncorrectMeetingId_ThenExceptionShouldBeThrown()
        {
            string validPerson = "Attendee3";
            int invalidId = 232135;

            Assert.ThrowsAny<Exception>(() => dbInMemory.AddAttendee(invalidId, validPerson));
        }

        [Fact]
        public void WhenAddingDuplicateAttendee_ThenReturnShouldBeFalse()
        {
            string duplicateAttendee = "Attendee2";
            int validId = 0;

            var response = dbInMemory.AddAttendee(validId, duplicateAttendee);
            Assert.False(response.isSuccess);
        }

        [Fact]
        public void WhenAddingDuplicateAttendee_ThenItShouldBeNotAddedToTheAttendeeList()
        {
            string duplicateAttendee = "Attendee2";
            int validId = 0;
            int oldAttendeeCount = db.Meeting[0].Attendees.Count;

            var response = dbInMemory.AddAttendee(validId, duplicateAttendee);
            Database updatedDatabase = new Database(dbInMemory);

            Assert.True(updatedDatabase.Meeting[0].Attendees.Count == oldAttendeeCount);
        }

        [Fact]
        public void WhenTryingToAddResponsiblePerson_ThenReturnShouldBeFalse()
        {
            string duplicateAttendee = "Person1";
            int validId = 0;

            var response = dbInMemory.AddAttendee(validId, duplicateAttendee);
            Assert.False(response.isSuccess);
        }

        [Fact]
        public void WhenTryingToAddResponsiblePerson_ThenItShouldBeNotAddedToTheAttendeeList()
        {
            string responsiblePerson = "Person1";
            int validId = 0;

            var response = dbInMemory.AddAttendee(validId, responsiblePerson);
            Database updatedDatabase = new Database(dbInMemory);

            Assert.False(updatedDatabase.Meeting[0].Attendees.Exists(p => p == responsiblePerson));
        }
    }
}
