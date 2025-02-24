using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Question
{
    public string Id { get; set; } = null!;

    public string? Gameid { get; set; }

    public string Questiontext { get; set; } = null!;

    public virtual Game? Game { get; set; }

    public virtual ICollection<Playeranswer> Playeranswers { get; set; } = new List<Playeranswer>();

    public virtual ICollection<Questionoption> Questionoptions { get; set; } = new List<Questionoption>();
}
