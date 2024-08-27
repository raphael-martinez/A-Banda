namespace Playmove.Core.API.Vms
{
    public class ArquivoVm
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public string Extensao { get; set; }
        public float Tamanho { get; set; }
        public string NomeFisico { get; }
        public string CaminhoArquivo { get; set; }
    }
}