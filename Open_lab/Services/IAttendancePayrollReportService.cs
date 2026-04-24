using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IAttendancePayrollReportService
    {
        Task<List<AttendancePayrollSummaryRow>> GeneratePayrollSummaryAsync(DateTime from, DateTime to, int? userId = null);
    }
}

