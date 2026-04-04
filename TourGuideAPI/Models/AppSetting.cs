using System;
using System.Collections.Generic;

namespace TourGuideAPI.Models;

public partial class AppSetting
{
    public int Id { get; set; }

    public string? SettingKey { get; set; }

    public string? SettingValue { get; set; }
}
