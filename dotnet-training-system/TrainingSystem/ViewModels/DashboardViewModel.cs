using TrainingSystem.Models;

namespace TrainingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int TotalRegistrations { get; set; }
        public int PendingRegistrations { get; set; }
        public int ApprovedRegistrations { get; set; }
        public int TotalBatches { get; set; }
        public int UpcomingBatches { get; set; }

        public List<TrainingProgram> RecentPrograms { get; set; } = new();
        public List<Registration> RecentRegistrations { get; set; } = new();
        public SystemSettings? Settings { get; set; }
        public string AdminName { get; set; } = string.Empty;
    }

    public class ProgramListViewModel
    {
        public List<TrainingProgram> Programs { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        public ProgramStatus? StatusFilter { get; set; }
        public List<string> AllCategories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class RegistrationListViewModel
    {
        public List<Registration> Registrations { get; set; } = new();
        public string? SearchQuery { get; set; }
        public RegistrationStatus? StatusFilter { get; set; }
        public int? ProgramFilter { get; set; }
        public List<TrainingProgram> AllPrograms { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string SortField { get; set; } = "RegisteredAt";
        public string SortDirection { get; set; } = "desc";
    }

    public class PublicProgramViewModel
    {
        public List<TrainingProgram> Programs { get; set; } = new();
        public SystemSettings? Settings { get; set; }
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        public List<string> AllCategories { get; set; } = new();
    }

    public class RegistrationFormViewModel
    {
        public TrainingProgram? Program { get; set; }
        public List<Batch> AvailableBatches { get; set; } = new();
        public Registration Registration { get; set; } = new();
    }
}
