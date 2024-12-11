using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StandCarros.Models;

public class Photo  
{  
    [Key]  
    public int Id { get; set; }  
    public byte[] Bytes { get; set; }  
    public string Description { get; set; }  
    public string FileExtension { get; set; }  
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Size { get; set; }  
    [ForeignKey("CarId")]
    public virtual Car Car { get; set; }  
} 

