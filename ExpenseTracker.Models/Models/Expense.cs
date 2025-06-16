using System;
using System.Collections.Generic;

namespace ExpenseTracker.Models.Models;

public partial class Expense
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ExpenseCategory? Category { get; set; }

    public virtual User? User { get; set; }
}
