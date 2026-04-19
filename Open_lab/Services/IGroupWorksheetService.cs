using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface IGroupWorksheetService
    {
        Task<List<WorkSheetPatientRow>> GetGroupWorksheetByGroupAsync(int groupId, DateTime from, DateTime to);
        Task<List<WorkSheetPatientRow>> GetGroupWorksheetByCustomGroupAsync(int customGroupId, DateTime from, DateTime to);
    }
}
