using System;
using Open_lab.Models;

namespace Open_lab.Tests.Infrastructure
{
    public static class TestSeed
    {
        public static Patient Patient(string labId = "LAB-001")
        {
            return new Patient
            {
                LabId = labId,
                FullName = "Test Patient",
                Gender = "Male",
                Phone = "01000000000",
                BirthDate = new DateTime(1990, 1, 1)
            };
        }

        public static Test Test(string code = "T-001", decimal price = 50m)
        {
            return new Test
            {
                Code = code,
                NameReport = code,
                NameReceipt = code,
                Price = price
            };
        }
    }
}

