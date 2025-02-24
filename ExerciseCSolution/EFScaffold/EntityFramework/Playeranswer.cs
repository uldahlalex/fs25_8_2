using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Playeranswer
{
    public string Playerid { get; set; } = null!;

    public string Questionid { get; set; } = null!;

    public string? Optionid { get; set; }

    public DateTime? Answertimestamp { get; set; }

    public virtual Questionoption? Option { get; set; }

    public virtual Player Player { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
