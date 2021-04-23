using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banchetta
{
    public class Operazione
    {
        public int IdOperazione { get; }
        public DateTime DataOperazione { get; }
        public string Causale { get; }
        public decimal Importo { get; }

        public override string ToString()
        {
            return $"{DataOperazione.ToShortDateString()}: {Importo:c}\t{Causale}";
        }

        public Operazione(int idOperazione, DateTime dataOperazione, string causale,
            decimal importo)
        {
            IdOperazione = idOperazione;
            DataOperazione = dataOperazione;
            Causale = causale;
            Importo = importo;
        }
    }
}
