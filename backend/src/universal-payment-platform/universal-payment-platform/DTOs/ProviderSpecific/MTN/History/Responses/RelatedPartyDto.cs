namespace Application.DTOs.Payments.Responses
{
    public record RelatedPartyDto(
        string? Id,
        string? Role,
        string? Name
    );
}
