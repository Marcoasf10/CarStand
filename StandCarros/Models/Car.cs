using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace StandCarros.Models;

public class Car
{
    [Key]
    public int Id { get; set; }
    [Display(Name="Nome")]
    public string Name { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public string Combustível { get; set; }
    [DataType(DataType.Date)]
    [Display(Name="Data de registo")]
    public DateTime DataRegisto { get; set; }
    [Display(Name="Quilómetros")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public int Quilometros { get; set; }
    [Display(Name="Cilindrada")]
    public int Cilindrada { get; set; }
    [Display(Name="Potência")]
    public int Potencia { get; set; }
    public string Cor { get; set; }
    public string Caixa { get; set; }
    [Display(Name="Nº de mudanças")]
    public int NumMudancas { get; set; }
    [Display(Name="Nº de portas")]
    public int NumPortas { get; set; }
    [Display(Name="Tração")]
    public string Tracao { get; set; }
    [Display(Name="Preço")]
    public int Preco { get; set; }
    [Display(Name="Descrição")]
    public string? Descricao { get; set; }
    [Display(Name="Fotos")]
    public virtual List<Photo> Photos { get; set; } = new();
    [FromForm] [NotMapped] public IFormFileCollection? Files { get; set; }
}