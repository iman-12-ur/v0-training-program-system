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
        public List<TrainingProgram> RecentPrograms { get; set; } = new();
        public List<Registration> RecentRegistrations { get; set; } = new();
        public string AdminWelcome { get; set; } = string.Empty;
        public string AdminDescription { get; set; } = string.Empty;
    }

    public class HomeViewModel
    {
        public List<TrainingProgram> Programs { get; set; } = new();
        public string WelcomeTitle { get; set; } = string.Empty;
        public string WelcomeDescription { get; set; } = string.Empty;
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        public List<string> AllCategories { get; set; } = new();
    }

    public class ProgramDetailsViewModel
    {
        public TrainingProgram Program { get; set; } = null!;
        public List<Batch> AvailableBatches { get; set; } = new();
    }

    public class RegisterViewModel
    {
        public TrainingProgram Program { get; set; } = null!;
        public Batch Batch { get; set; } = null!;
        public Registration Registration { get; set; } = new();
    }
}
