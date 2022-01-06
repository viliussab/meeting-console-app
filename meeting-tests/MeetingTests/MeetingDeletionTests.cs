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
    public class MeetingDeletionTests
    {
        private readonly Database db;
        private readonly DbInMemory dbInMemory;

        public MeetingDeletionTests()
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
            db.Meeting.Add(
                new Meeting(1,
                "Meeting",
                "Person1",
                "No description",
                Category.CodeMonkey,
                Type.InPerson, DateTime.Now,
                DateTime.Now + new TimeSpan(2, 0, 0),
                new()));

            dbInMemory = new DbInMemory(db);
        }

        [Fact]
        public void WhenDeletingMeetingWithRightId_ThenTheResponseShouldBeTrue()
        {
            int idOfFirstElement = db.Meeting[0].Id;

            var response = dbInMemory.DeleteMeeting(idOfFirstElement);
            Assert.True(response.isSuccess);
        }

        [Fact]
        public void WhenDeletingMeetingWithRightId_ThenTheMeetingShouldBeDeleted()
        {
            int idOfFirstElement = db.Meeting[0].Id;

            var response = dbInMemory.DeleteMeeting(idOfFirstElement);
            Database updatedDatabase = new Database(dbInMemory);

            Assert.False(updatedDatabase.Meeting.Exists(m => m.Id == idOfFirstElement));
        }

        [Fact]
        public void WhenDeletingMeetingWithWrongId_ThenResponseShouldBeError()
        {
            int idOfFirstElement = 321312;

            var response = dbInMemory.DeleteMeeting(idOfFirstElement);
            Assert.False(response.isSuccess);
        }
    }
}
