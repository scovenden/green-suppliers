namespace GreenSuppliers.Api.Models.DTOs;

public class ProfileAnalyticsDto
{
    public int TotalViews { get; set; }
    public int ViewsThisMonth { get; set; }
    public List<ViewsByDayDto> ViewsByDay { get; set; } = new();
    public int TotalLeads { get; set; }
    public List<LeadsByMonthDto> LeadsByMonth { get; set; } = new();
}

public class ViewsByDayDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class LeadsByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}
