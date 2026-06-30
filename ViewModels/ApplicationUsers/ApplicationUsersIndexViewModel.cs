using Project.Models;

namespace Project.ViewModels.ApplicationUsers
{
    public class ApplicationUsersIndexViewModel
    {
        public IReadOnlyList<ApplicationUser> Users { get; init; } = Array.Empty<ApplicationUser>();
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; }
        public int TotalPages { get; init; }
        public int TotalUsers { get; init; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
