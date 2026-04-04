using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class Audio
{
    public int Id { get; set; }

    public int? PoiId { get; set; }

    public int? LanguageId { get; set; }

    public string? AudioUrl { get; set; }

    public int? Duration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Language? Language { get; set; }

    public virtual Poi? Poi { get; set; }
}
