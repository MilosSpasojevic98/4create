using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicalTrialApp.Models;

public class ClinicalTrialMetadata
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string TrialId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Participants { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrialStatus Status { get; set; }

    public int? DurationInDays { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public enum TrialStatus
{
    NotStarted,
    Ongoing,
    Completed
} 