using System;
using System.Collections.Generic;

namespace WebBanNuocMVC.Data;

public partial class RevenueReport
{
    public int ReportId { get; set; }

    public DateOnly? ReportDate { get; set; }

    public decimal? TotalRevenue { get; set; }
}
