using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class Image
{
    public int Id { get; set; }

    public int? PoiId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Caption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Poi? Poi { get; set; }
}
