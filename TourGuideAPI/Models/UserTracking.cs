using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class UserTracking
{
    public int Id { get; set; }

    public string? DeviceId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? PoiId { get; set; }

    public DateTime? VisitTime { get; set; }

    public virtual Poi? Poi { get; set; }
}
