using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

public class TimeRegistrationFormModel
{
    [Required(ErrorMessage = "Vælg en sag.")]
    public int? WorkCaseId { get; set; }

    [Required(ErrorMessage = "Vælg en opgave.")]
    public int? OrderId { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Angiv hvor mange timer du har arbejdet.")]
    [Range(0.25, 24, ErrorMessage = "Timer skal være mellem 0,25 og 24.")]
    public decimal? Hours { get; set; }

    [MaxLength(500, ErrorMessage = "Kommentaren må højst være 500 tegn.")]
    public string? Comment { get; set; }
}
