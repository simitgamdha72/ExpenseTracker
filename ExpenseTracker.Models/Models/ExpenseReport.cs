using System;
using System.Collections.Generic;

namespace ExpenseTracker.Models.Models;

public partial class ExpenseReport
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
