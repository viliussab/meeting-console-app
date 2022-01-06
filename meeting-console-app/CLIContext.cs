﻿using meeting_console_app.CLIViews;
using meeting_console_app.CLIViews.MeetingViews;
using meeting_console_app.Data;
using meeting_console_app.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meeting_console_app
{
    public class CLIContext
    {
        static readonly object _lock = new object();
        public DbInMemory DbInMemory { get { return _database; } }

        public void Run()
        {
            lock(_lock)
            {
                if (_isRun)
                    throw new Exception($"Illegal call: trying to run the {this.GetType().Name} twice");
                _isRun = true;
            }

            try
            {
                LoadJsonDatabase(Program.JsonFileName);
                IEnumerable<Meeting> meetings;
                _database.GetMeetings(out meetings);
                _currentView = new MainMeetingView(this);
                LoopViews();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught. Aborting the {1}.", ex, this.GetType().Name);
            }
            finally
            {
                SaveJsonDatabase(Program.JsonFileName);
            }
        }

        public void ChangeView(CLIView view)
        {
            _currentView = view;
        }

        private CLIView _currentView;
        private DbInMemory _database;
        private bool _isRun = false;

        private void LoadJsonDatabase(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateJsonFile(filePath);
            }
            string data = System.IO.File.ReadAllText(Program.JsonFileName);
            Database database = JsonConvert.DeserializeObject<Database>(data);
            if (database == null)
                throw new IOException($"Could not parse the data of file {Program.JsonFileName}");

            _database = new DbInMemory(database);
        }

        private void CreateJsonFile(string filePath)
        {
            Database database = new Database();
            string data = JsonConvert.SerializeObject(database);
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(data);
            }
        }

        private void SaveJsonDatabase(string filePath)
        {
            if (_database == null)
                throw new Exception("Trying to save Json into the file but database parameter is found to be null");

            string json = JsonConvert.SerializeObject(new Database(_database), Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(json);
            }
        }

        private void LoopViews()
        {
            while (_currentView != null)
            {
                Console.Clear();
                if (!String.IsNullOrEmpty(_currentView.FailureMsg))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(_currentView.FailureMsg + "\n");
                    Console.ResetColor();
                }
                if (!String.IsNullOrEmpty(_currentView.SuccessMsg))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(_currentView.SuccessMsg + "\n");
                    Console.ResetColor();
                }

                Console.Write(_currentView.GetContents());

                Console.Write("Your command: ");
                var line = Console.ReadLine();
                var parts = line.Split(' ');
                if (parts.Length > 1)
                {
                    _currentView = _currentView.CallCommand(parts[0], parts[1..]);
                }
                else
                    _currentView = _currentView.CallCommand(parts[0], Array.Empty<string>());


            }

        }
    }
}
