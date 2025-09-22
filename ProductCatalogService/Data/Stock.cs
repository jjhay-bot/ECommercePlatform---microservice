using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalogService.Data;

public class Stock
{
  public int Id { get; set; }
  public string Symbol { get; set; } = string.Empty;
  public string CompanyName { get; set; } = string.Empty;
  [Column(TypeName = "decimal(18,2)")]
  public decimal Purchase { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal LastDiv { get; set; }
  public string Industry { get; set; } = string.Empty;
  public string MarketCap { get; set; } = string.Empty;
  public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
