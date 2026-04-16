using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class CultureSensitivityValue
    {
        public int AntibioticId { get; set; }
        public string Sensitivity { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    public interface ICultureSensitivityService
    {
        Task<List<Culture>> GetCulturesAsync();
        Task<Culture> CreateCultureAsync(Culture culture);
        Task DeleteCultureAsync(int cultureId);

        Task<List<Antibiotic>> GetAntibioticsAsync();
        Task<Antibiotic> CreateAntibioticAsync(Antibiotic antibiotic);
        Task DeleteAntibioticAsync(int antibioticId);

        Task<List<CultureAntibiotic>> GetCultureAntibioticsAsync(int cultureId);
        Task LinkAntibioticAsync(int cultureId, int antibioticId);
        Task UnlinkAntibioticAsync(int cultureId, int antibioticId);

        Task<List<CultureVisitTestRow>> SearchCultureVisitTestsAsync(string? labId, DateTime from, DateTime to);
        Task SaveCultureResultAsync(int visitTestId, int cultureId, IReadOnlyCollection<CultureSensitivityValue> sensitivities);
    }
}
