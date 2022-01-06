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
    public class MeetingAttendeeRemoveTests
    {
        private readonly Database db;
        private readonly DbInMemory dbInMemory;

        public MeetingAttendeeRemoveTests()
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
        public void WhenRemovingCorrectAttendee_ThenReturnShouldBeTrue()
        {
            string validPerson = "Attendee2";
            int validId = 0;

            var response = dbInMemory.RemoveAttendee(validId, validPerson);
            Assert.True(response.isSuccess);
        }

        [Fact]
        public void WhenRemovingCorrectAttendee_ThenItShouldBeRemovedFromTheAttendeeList()
        {
            string validPerson = "Attendee2";
            int validId = 0;

            int oldAttendeeCount = db.Meeting[0].Attendees.Count;

            var response = dbInMemory.RemoveAttendee(validId, validPerson);
            Database updatedDatabase = new Database(dbInMemory);

            Assert.False(updatedDatabase.Meeting[0].Attendees.Exists(p => p == validPerson));
        }

        [Fact]
        public void WhenRemovingAttendeeWithIncorrectMeetingId_ThenExceptionShouldBeThrown()
        {
            string validPerson = "Attendee2";
            int invalidId = 232135;

            Assert.ThrowsAny<Exception>(() => dbInMemory.AddAttendee(invalidId, validPerson));
        }

        [Fact]
        public void WhenRemovingNonExistantAttendee_ThenReturnShouldBeFalse()
        {
            string invalidPerson = "Attendee3";
            int validId = 0;

            var response = dbInMemory.RemoveAttendee(validId, invalidPerson);
            Assert.False(response.isSuccess);
        }

        [Fact]
        public void WhenRemovingNonExistantAttendee_ThenTheAttendeeListShouldNotChange()
        {
            string invalidPerson = "Attendee3";
            int validId = 0;
            int oldAttendeeCount = db.Meeting[0].Attendees.Count;

            var response = dbInMemory.RemoveAttendee(validId, invalidPerson);

            Database updatedDatabase = new Database(dbInMemory);
            Assert.True(updatedDatabase.Meeting[0].Attendees.Count == oldAttendeeCount);
        }

        [Fact]
        public void WhenTryingToRemoveResponsiblePerson_ThenReturnShouldBeFalse()
        {
            string responsiblePerson = "Person1";
            int validId = 0;


            var response = dbInMemory.RemoveAttendee(validId, responsiblePerson);
            
            Assert.False(response.isSuccess);
        }

        [Fact]
        public void WhenTryingToRemoveResponsiblePerson_ThenTheAttendeeListShouldNotChange()
        {
            string responsiblePerson = "Person1";
            int validId = 0;
            int oldAttendeeCount = db.Meeting[0].Attendees.Count;

            dbInMemory.RemoveAttendee(validId, responsiblePerson);
            Database updatedDatabase = new Database(dbInMemory);
            Assert.True(updatedDatabase.Meeting[0].Attendees.Count == oldAttendeeCount);
        }
    }
}
