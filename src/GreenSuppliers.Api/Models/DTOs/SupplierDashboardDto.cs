namespace GreenSuppliers.Api.Models.DTOs;

public class SupplierDashboardDto
{
    public int LeadCount { get; set; }
    public int NewLeadCount { get; set; }
    public int CertificationCount { get; set; }
    public int PendingCertCount { get; set; }
    public string EsgLevel { get; set; } = string.Empty;
    public int EsgScore { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int ProfileCompleteness { get; set; }
}
