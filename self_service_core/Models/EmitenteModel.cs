using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace self_service_core.Models;

public class EmitenteModel
{
    [BsonId]
    [JsonIgnore]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RazaoSocial { get; set; }
    public string NomeFantasia { get; set; }
    public string Cnpj { get; set; }
    public string InscricaoEstadual { get; set; }
    public string InscricaoMunicipal { get; set; }
    public string Cep { get; set; }
    public string Endereco { get; set; }
    public string Numero { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string Telefone { get; set; }
    
    public EmitenteModel(string razaoSocial, string nomeFantasia, string cnpj, string inscricaoEstadual, string inscricaoMunicipal, string cep, string endereco, string numero, string bairro, string cidade, string estado, string telefone)
    {
        this.RazaoSocial = razaoSocial;
        this.NomeFantasia = nomeFantasia;
        this.Cnpj = cnpj;
        this.InscricaoEstadual = inscricaoEstadual;
        this.InscricaoMunicipal = inscricaoMunicipal;
        this.Cep = cep;
        this.Endereco = endereco;
        this.Numero = numero;
        this.Bairro = bairro;
        this.Cidade = cidade;
        this.Estado = estado;
        this.Telefone = telefone;
    }
    
    
}