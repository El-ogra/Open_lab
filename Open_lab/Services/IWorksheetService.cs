using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface IWorksheetService
    {
        Task<List<WorkSheetPatientRow>> GetWorksheetByPatientAsync(DateTime from, DateTime to);
        Task<List<WorkSheetTestRow>> GetWorksheetByTestAsync(DateTime from, DateTime to);
    }
}
