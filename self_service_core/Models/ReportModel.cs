namespace self_service_core.Models;

public class ReportModel
{
    
    public List<ReportItemModel> MostSoldItems { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public double AverageTicket { get; set; }
    public double AverageOrderByDay { get; set; }
}