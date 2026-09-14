namespace JitOverheadDemo.Models
{
    public class JitDemoLegadoDto
    {
        public string Compilador { get; set; }
        public double MemoriaWorkingSetMb { get; set; }
        public bool CompilacaoJitAtiva { get; set; }
        public string Serializador { get; set; }
        public string VersaoClr { get; set; }
    }
}
