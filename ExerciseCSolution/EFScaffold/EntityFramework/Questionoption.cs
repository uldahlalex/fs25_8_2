using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Questionoption
{
    public string Id { get; set; } = null!;

    public string? Questionid { get; set; }

    public string Optiontext { get; set; } = null!;

    public bool Iscorrect { get; set; }

    public virtual ICollection<Playeranswer> Playeranswers { get; set; } = new List<Playeranswer>();

    public virtual Question? Question { get; set; }
}
