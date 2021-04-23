using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Banchetta
{
    public static class Banca  // una sorta di Data Access Layer
    {
        static string _connectionString = ConfigurationManager.ConnectionStrings["Banchetta"].ConnectionString;

        // Elimina un conto e tutte le sue operazioni in modalità disconnessa
        // Solo per esercizio usiamo un dataset
        public static void EliminaConto(Conto c)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlDataAdapter da = new SqlDataAdapter("Select * from Conti", conn))
            {
                DataSet ds = new DataSet();

                da.Fill(ds, "Conti");

                DataTable tableConti = ds.Tables["Conti"];

                tableConti.PrimaryKey = new DataColumn[] { tableConti.Columns["IdConto"] };

                tableConti.Rows.Find(c.IdConto).Delete();

                //foreach (DataRow row in tableConti.Rows)
                //    if ((int)row["IdConto"] == c.IdConto)
                //    {
                //        row.Delete();
                //        break;
                //    }

                new SqlCommandBuilder(da);  // crea i comandi per l'update del db

                da.Update(tableConti);
            }
        }

        // restituisce una lista di tutti i conti con eventualemente le operazioni
        public static List<Conto> ElencoConti(bool conOperazioni = false)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("Select * from Conti", conn))
            {
                List<Conto> conti = new List<Conto>();

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    conti.Add(new Conto((int)reader["IdConto"], reader["Intestatario"].ToString()));

                conn.Close();

                if (conOperazioni)
                    foreach (Conto c in conti)
                        RecuperaOperazioni(c);

                return conti;
            }
        }
        public static Conto CreaConto(string intestatario)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("Insert into Conti (Intestatario) values (@intestatario); Select @@identity", conn))
            {
                cmd.Parameters.AddWithValue("@intestatario", intestatario);
                conn.Open();

                try
                {
                    int id = (int)(decimal)cmd.ExecuteScalar();
                    return new Conto(id, intestatario);
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("IntestatarioNonVuoto"))
                        throw new ArgumentNullException("L'intestatario deve essere valorizzato");
                }
            }

            return null;
        }

        public static void PrelevaDalConto(Conto conto, DateTime dataOperazione, decimal importo, string causale)
        {
            VersaSulConto(conto, dataOperazione, -importo, causale);
        }

        public static void VersaSulConto(Conto conto, DateTime dataOperazione, decimal importo, string causale)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("Insert into Operazioni (DataOperazione, Causale, Importo, IdConto) " +
                "values (@dataOperazione, @causale, @importo, @idConto); Select @@identity", conn))
            {
                cmd.Parameters.AddWithValue("@dataOperazione", dataOperazione);
                cmd.Parameters.AddWithValue("@causale", causale);
                cmd.Parameters.AddWithValue("@importo", importo);
                cmd.Parameters.AddWithValue("@idConto", conto.IdConto);

                conn.Open();
                int id = (int)(decimal)cmd.ExecuteScalar();

                Operazione op = new Operazione(id, dataOperazione, causale, importo);

                conto.Operazioni.Add(op);
            }
        }

        // recupera le operazioni del conto e le aggiunge alla sua lista di operazioni
        public static void RecuperaOperazioni(Conto conto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("Select * from Operazioni where IdConto = @idConto", conn))
            {
                cmd.Parameters.AddWithValue("@idConto", conto.IdConto);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                conto.Operazioni.Clear();
                // pulisco la lista di operazioni
                // prima di recuperarle dal db

                while (reader.Read())
                    conto.Operazioni.Add(
                        new Operazione((int)reader["IdOperazione"],
                            (DateTime)reader["DataOperazione"],
                            reader["Causale"].ToString(),
                            (decimal)reader["Importo"]));
            }
        }

        public static Conto RecuperaConto(int idConto, bool conOperazioni = false)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("RecuperaConto", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idConto", idConto);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read()) // se non sono stati restituiti records
                    return null;

                Conto c = new Conto((int)reader["IdConto"], reader["Intestatario"].ToString());

                conn.Close();   // non necessario in quanto sto usando using

                if (conOperazioni)
                    RecuperaOperazioni(c);

                return c;
            }
        }
    }
}
