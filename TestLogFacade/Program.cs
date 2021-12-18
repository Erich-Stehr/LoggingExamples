using System;

namespace TestLogFacade
{
    class Program
    {
        static void Main(string[] args)
        {
            FakeLogger fake = new FakeLogger();

            fake.Log("Testing");
            fake.Log(new ArgumentException("Args not checked", nameof(args)));

            foreach (var entry in fake)
                Console.WriteLine($"{entry.DateTime:o}:{entry.Severity}:{entry.Message}::{entry.Exception}");
        }
    }
}
