using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

// Limits match the backend's Task entity (title 200, description 2000).
public class TaskFormModel
{
    [Required(ErrorMessage = "Vælg en sag.")]
    public Guid? OrderId { get; set; }

    [Required(ErrorMessage = "Angiv en titel.")]
    [MaxLength(200, ErrorMessage = "Titlen må højst være 200 tegn.")]
    public string? Title { get; set; }

    [MaxLength(2000, ErrorMessage = "Beskrivelsen må højst være 2000 tegn.")]
    public string? Description { get; set; }
}
