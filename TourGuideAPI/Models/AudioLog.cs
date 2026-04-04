using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class AudioLog
{
    public int Id { get; set; }

    public string? DeviceId { get; set; }

    public int? PoiId { get; set; }

    public int? LanguageId { get; set; }

    public DateTime? PlayTime { get; set; }

    public virtual Language? Language { get; set; }

    public virtual Poi? Poi { get; set; }
}
