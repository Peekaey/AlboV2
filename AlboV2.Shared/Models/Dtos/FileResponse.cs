namespace AlboV2.Shared.Models.Dtos;

public record FileResponse(Stream content, string fileName, string? contentType = null);