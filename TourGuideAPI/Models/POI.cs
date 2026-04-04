using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class Poi
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int? Radius { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AudioLog> AudioLogs { get; set; } = new List<AudioLog>();

    public virtual ICollection<Audio> Audios { get; set; } = new List<Audio>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();

    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();

    public virtual ICollection<UserTracking> UserTrackings { get; set; } = new List<UserTracking>();
}
