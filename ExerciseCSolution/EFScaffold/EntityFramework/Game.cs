using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Game
{
    public string Id { get; set; } = null!;

    public string? Template { get; set; }

    public virtual ICollection<Gameround> Gamerounds { get; set; } = new List<Gameround>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual Gametemplate? TemplateNavigation { get; set; }
}
