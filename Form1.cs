using RicercaOperativa.Classes;

namespace RicercaOperativa
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int nDestinazioni = 0;
        int nUnita = 0;


        #region Eventi Pulsanti

        private void btn_crea_matrice_Click(object sender, EventArgs e)
        {
            CreaMatrice();
        }

        private void btn_dati_casuali_Click(object sender, EventArgs e)
        {
            DatiCasuali();
        }

        private async void btn_calcola_nordovest_Click(object sender, EventArgs e)
        {
            if (dgv_nordovest.Rows.Count == 0 && dgv_nordovest.Columns.Count == 0)
            {
                MessageBox.Show("Creare una tabella con dati prima di calcolare.");
                return;
            }

            foreach (DataGridViewRow row in dgv_nordovest.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null)
                    {
                        MessageBox.Show("Necessario riempire tutte le celle per calcolare");
                        return;
                    }
                }
            }

            if(MinimiCosti.InExecution == true)
            {
                MessageBox.Show("Non è consentito eseguire due algoritmi contemporaneamente. \n Attendere il termine dell'esecuzione");
                return;
            }

            Task.Run(() => { CalcolaNordOvest(); });
            NordOvest.InExecution = true;
        }

        private async void btn_calcola_minimicosti_Click(object sender, EventArgs e)
        {
            if (dgv_minimicosti.Rows.Count == 0 && dgv_minimicosti.Columns.Count == 0)
            {
                MessageBox.Show("Creare una tabella con dati prima di calcolare.");
                return;
            }

            foreach (DataGridViewRow row in dgv_minimicosti.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null)
                    {
                        MessageBox.Show("Necessario riempire tutte le celle per calcolare");
                        return;
                    }
                }
            }

            if (NordOvest.InExecution == true)
            {
                MessageBox.Show("Non è consentito eseguire due algoritmi contemporaneamente. \n Attendere il termine dell'esecuzione");
                return;
            }


            Task.Run(() => { CalcolaMinimiCosti(); });
            MinimiCosti.InExecution = true;
        }

        #endregion


        #region Metodi Matrice

        private void CreaMatrice()
        {
            nDestinazioni = Convert.ToInt32(nupd_destinazioni.Value);
            nUnita = Convert.ToInt32(nupd_unita.Value);

            //resetto ogni datagridview
            ResettaDgv(dgv_nordovest);
            ResettaDgv(dgv_minimicosti);

            //creo le matrici con anche gli headers
            CreaDgv(dgv_nordovest, nDestinazioni, nUnita);
            CreaDgv(dgv_minimicosti, nDestinazioni, nUnita);
        }

        private void DatiCasuali()
        {
            var rnd = new Random();
            int sum = rnd.Next(80, 120) * nDestinazioni;

            int[] totDest = Calcoli.TotaliCasuali(nDestinazioni, sum);
            int[] totUp = Calcoli.TotaliCasuali(nUnita, sum);

            AssegnaTotali(dgv_nordovest, totDest, totUp, sum);
            AssegnaTotali(dgv_minimicosti, totDest, totUp, sum);

            AssegnaValoriCosto();
        }


        private void AssegnaTotali(DataGridView dgv, int[] totDest, int[] totUp, int sum)
        {
            //valori totali destinazioni
            for(int i = 0; i < nDestinazioni; i++)
            {
                dgv[i, nUnita].Value = totDest[i];
                dgv[i, nUnita].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            //valori totali unità
            for(int i = 0; i < nUnita; i++)
            {
                dgv[nDestinazioni, i].Value = totUp[i];
                dgv[nDestinazioni, i].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgv[nDestinazioni, nUnita].Value = sum;
            dgv[nDestinazioni, nUnita].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void AssegnaValoriCosto()
        {
            for(int i = 0; i < nUnita; i++)
            {
                for(int j = 0; j < nDestinazioni; j++)
                {
                    var rnd = new Random();
                    int val = rnd.Next(20, 60);

                    dgv_nordovest[j, i].Value = val;
                    dgv_minimicosti[j, i].Value = val;
                    dgv_nordovest[j, i].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgv_minimicosti[j, i].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void CreaDgv(DataGridView dgv,int dest, int up)
        {
            //aggiungo +1 perchè considero riga/colonna totali
            dgv.RowCount = up + 1;
            dgv.ColumnCount = dest + 1;

            //assegno gli headers

            //colonne
            for(int i = 0; i < dest; i++)
            {
                dgv.Columns[i].HeaderText = "D" + (i + 1);
            }
            dgv.Columns[dest].HeaderText = "TOT";

            //righe
            for(int i = 0; i < up; i++)
            {
                dgv.Rows[i].HeaderCell.Value = "UP" + (i + 1);
            }
            dgv.Rows[up].HeaderCell.Value = "TOT";
        }

        private void ResettaDgv(DataGridView dgv)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            dgv.Refresh();
        }

        #endregion


        #region Algoritmi

        private async Task CalcolaNordOvest()
        {
            NordOvest.CostoTotale = 0;
            txt_log.ResetText();

            NordOvest.Velocita = Convert.ToInt32(nupd_velocita_nordovest.Value);

            txt_log.Text += "ALGORITMO NORD-OVEST";

            while(dgv_nordovest.Rows.Count != 1 && dgv_nordovest.Columns.Count != 1)
            {
                int totDest = Convert.ToInt32(dgv_nordovest[0, dgv_nordovest.Rows.Count-1].Value);
                int totUp = Convert.ToInt32(dgv_nordovest[dgv_nordovest.Columns.Count-1, 0].Value);

                if(totUp > totDest)
                {
                    int newValue = totUp - totDest;
                    int costo = Convert.ToInt32(dgv_nordovest[0, 0].Value);                    

                    //prints log
                    string unita = dgv_nordovest.Rows[0].HeaderCell.Value.ToString();
                    string destinazione = dgv_nordovest.Columns[0].HeaderText;

                    int quantità = totDest;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    NordOvest.CostoTotale += costoTotale;

                    dgv_nordovest.Columns.RemoveAt(0);
                    dgv_nordovest[dgv_nordovest.Columns.Count - 1, 0].Value = newValue;
                    dgv_nordovest.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value);
                    totValue = totValue - totDest;
                    dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value = totValue;

                    Thread.Sleep(NordOvest.Velocita);
                    continue;
                }

                if(totUp < totDest)
                {
                    int newValue = totDest - totUp;
                    int costo = Convert.ToInt32(dgv_nordovest[0, 0].Value);

                    //prints log
                    string unita = dgv_nordovest.Rows[0].HeaderCell.Value.ToString();
                    string destinazione = dgv_nordovest.Columns[0].HeaderText;
                    
                    int quantità = totUp;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    NordOvest.CostoTotale += costoTotale;

                    dgv_nordovest.Rows.RemoveAt(0);
                    dgv_nordovest[0, dgv_nordovest.Rows.Count - 1].Value = newValue;
                    dgv_nordovest.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value);
                    totValue = totValue - totUp;
                    dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value = totValue;

                    Thread.Sleep(NordOvest.Velocita);
                    continue;
                }

                if(totUp == totDest)
                {
                    int costo = Convert.ToInt32(dgv_nordovest[0, 0].Value);

                    //prints log
                    string unita = dgv_nordovest.Rows[0].HeaderCell.Value.ToString();
                    string destinazione = dgv_nordovest.Columns[0].HeaderText;

                    int quantità = totUp;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    NordOvest.CostoTotale += costoTotale;

                    dgv_nordovest.Rows.RemoveAt(0);
                    dgv_nordovest.Columns.RemoveAt(0);
                    dgv_nordovest.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value);
                    totValue = totValue - totUp;
                    dgv_nordovest[dgv_nordovest.ColumnCount - 1, dgv_nordovest.RowCount - 1].Value = totValue;

                    Thread.Sleep(NordOvest.Velocita);
                    continue;
                }
                             
            }

            NordOvest.InExecution = false;

            //stampa il costo totale
            txt_log.Text += Environment.NewLine + $"Costo totale: {NordOvest.CostoTotale}€";

            txt_log.Text += Environment.NewLine + $"----------------------------------";
        }

        private async Task CalcolaMinimiCosti()
        {
            MinimiCosti.CostoTotale = 0;
            txt_log.ResetText();

            MinimiCosti.Velocita = Convert.ToInt32(nupd_velocita_minimicosti.Value);

            txt_log.Text += "ALGORITMO MINIMI COSTI";

            //IL PROBLEMA E CHE ALL'ULTIMA ITERAZIONE PER QUALCHE MOTIVO RANDOM NON DIMINUISCE IL VALORE DEL TOTALE

            while (dgv_minimicosti.Rows.Count > 1 && dgv_minimicosti.Columns.Count > 1)
            {
                int[] cell = Calcoli.TrovaCostoMinore(dgv_minimicosti);
                int cellColumnIndex = cell[0];
                int cellRowIndex = cell[1];

                int totUp = Convert.ToInt32(dgv_minimicosti[dgv_minimicosti.Columns.Count - 1, cellRowIndex].Value);
                int totDest = Convert.ToInt32(dgv_minimicosti[cellColumnIndex, dgv_minimicosti.Rows.Count - 1].Value);

                if (totUp > totDest)
                {
                    int newValue = totUp - totDest;
                    dgv_minimicosti[dgv_minimicosti.Columns.Count - 1, cellRowIndex].Value = newValue;           

                    //prints log
                    string unita = dgv_minimicosti.Rows[cellRowIndex].HeaderCell.Value.ToString();
                    string destinazione = dgv_minimicosti.Columns[cellColumnIndex].HeaderText;
                    int costo = Convert.ToInt32(dgv_minimicosti[cellColumnIndex, cellRowIndex].Value);

                    int quantità = totDest;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    MinimiCosti.CostoTotale += costoTotale;

                    dgv_minimicosti.Columns.RemoveAt(cellColumnIndex);
                    dgv_minimicosti.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value);
                    totValue = totValue - totDest;
                    dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value = totValue;

                    Thread.Sleep(MinimiCosti.Velocita);
                    continue;
                }

                if (totUp < totDest)
                {
                    int newValue = totDest - totUp;
                    dgv_minimicosti[cellColumnIndex, dgv_minimicosti.Rows.Count - 1].Value = newValue;                    

                    //prints log
                    string unita = dgv_minimicosti.Rows[cellRowIndex].HeaderCell.Value.ToString();
                    string destinazione = dgv_minimicosti.Columns[cellColumnIndex].HeaderText;
                    int costo = Convert.ToInt32(dgv_minimicosti[cellColumnIndex, cellRowIndex].Value);

                    int quantità = totUp;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    MinimiCosti.CostoTotale += costoTotale;

                    dgv_minimicosti.Rows.RemoveAt(cellRowIndex);
                    dgv_minimicosti.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value);
                    totValue = totValue - totUp;
                    dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value = totValue;

                    Thread.Sleep(MinimiCosti.Velocita);
                    continue;
                }

                if (totUp == totDest)
                {
                    //prints log
                    string unita = dgv_minimicosti.Rows[cellRowIndex].HeaderCell.Value.ToString();
                    string destinazione = dgv_minimicosti.Columns[cellColumnIndex].HeaderText;
                    int costo = Convert.ToInt32(dgv_minimicosti[cellColumnIndex, cellRowIndex].Value);

                    int quantità = totUp;
                    int costoTotale = costo * quantità;
                    txt_log.Text += Environment.NewLine + $"{unita} => {destinazione}; Quantità: {quantità}, costo: {costoTotale}€";
                    MinimiCosti.CostoTotale += costoTotale;

                    dgv_minimicosti.Columns.RemoveAt(cellColumnIndex);
                    dgv_minimicosti.Rows.RemoveAt(cellRowIndex);
                    dgv_minimicosti.Refresh();

                    //riduco il valore del totale
                    int totValue = Convert.ToInt32(dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value);
                    totValue = totValue - totUp;
                    dgv_minimicosti[dgv_minimicosti.ColumnCount - 1, dgv_minimicosti.RowCount - 1].Value = totValue;
                }

            }

            MinimiCosti.InExecution = false;

            //stampa il costo totale
            txt_log.Text += Environment.NewLine + $"Costo totale: {MinimiCosti.CostoTotale}€";

            txt_log.Text += Environment.NewLine + $"----------------------------------";

        }



        #endregion

        private void Form1_Shown(object sender, EventArgs e)
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }
    }
}