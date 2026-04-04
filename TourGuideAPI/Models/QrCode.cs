using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class QrCode
{
    public int Id { get; set; }

    public int? PoiId { get; set; }

    public string? QrValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Poi? Poi { get; set; }
}
