using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

public class TimeRegistrationFormModel
{
    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Enter how many hours you worked.")]
    [Range(0.25, 24, ErrorMessage = "Hours must be between 0.25 and 24.")]
    public decimal? Hours { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
