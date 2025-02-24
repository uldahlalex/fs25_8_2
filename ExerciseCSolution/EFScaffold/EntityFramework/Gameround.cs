using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Gameround
{
    public string? Gameid { get; set; }

    public string? Roundquestionid { get; set; }

    public string Id { get; set; } = null!;

    public virtual Game? Game { get; set; }

    public virtual Question? Roundquestion { get; set; }
}
