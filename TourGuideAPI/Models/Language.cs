using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class Language
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<AudioLog> AudioLogs { get; set; } = new List<AudioLog>();

    public virtual ICollection<Audio> Audios { get; set; } = new List<Audio>();

    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
}
