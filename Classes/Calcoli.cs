using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicercaOperativa.Classes
{
    internal class Calcoli
    {
        public static int[] TotaliCasuali(int n, int sum)
        {
            int[] arr = new int[n];

            for (int i = 0; i < sum; i++)
            {
                var rnd = new Random();

                arr[rnd.Next(1, sum) % n]++;
            }

            return arr;
        }

        public static int[] TrovaCostoMinore(DataGridView dgv)
        {
            //0 = cell, 1 = row
            int[] result = new int[2];

            int min = Convert.ToInt32(dgv[0, 0].Value);
            result[0] = 0;
            result[1] = 0;

            //foreach (DataGridViewRow row in dgv.Rows)
            //{
            //    foreach (DataGridViewCell cell in row.Cells)
            //    {
            //        if(Convert.ToInt32(cell.Value) < min)
            //        {
            //            min = Convert.ToInt32(cell.Value);

            //            result[0] = cell.ColumnIndex;
            //            result[1] = cell.RowIndex;
            //        }
            //    }
            //}

            for(int i = 0; i < dgv.RowCount - 1; i++)
            {
                for(int j = 0; j < dgv.ColumnCount -1; j++)
                {
                    DataGridViewCell cell = dgv[j, i];

                    if(Convert.ToInt32(cell.Value) < min)
                    {
                        min = Convert.ToInt32(cell.Value);

                        result[0] = cell.ColumnIndex;
                        result[1] = cell.RowIndex;
                    }
                }
            }

            return result;
        }
    }
}
