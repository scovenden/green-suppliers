namespace GreenSuppliers.Api.Models.DTOs;

public class SdgDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class UpdateSdgsRequest
{
    public List<int> SdgIds { get; set; } = new();
}
