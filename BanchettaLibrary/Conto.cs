using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banchetta
{
    public class Conto
    {
        public int IdConto { get; }
        public string Intestatario { get; }

        public Conto(int idConto, string intestatario)
        {
            IdConto = idConto;
            Intestatario = intestatario;
        }

        public override string ToString()
        {
            return $"{IdConto}: {Intestatario}";
        }

        public decimal Saldo { get
            {
                decimal s = 0;
                foreach (Operazione o in Operazioni)
                    s += o.Importo;
                
                return s;
            }
        }

        public List<Operazione> Operazioni { get; } = new List<Operazione>();
    }
}
