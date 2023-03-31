using UnityEngine;

public class ReadData
{
    string[,] data;
    int lineSize, rowSize;

    public string[,] GetText(TextAsset txt)
    {
        string currentText = txt.text.Substring(0, txt.text.Length - 1);
        string[] line = currentText.Split('\n');
        lineSize = line.Length;
        rowSize = line[0].Split('\t').Length;
        data = new string[lineSize, rowSize];

        for (int i = 0; i < lineSize; i++)
        {
            string[] row = line[i].Split('\t');
            for (int ii = 0; ii < rowSize; ii++)
            {
                if(ii == rowSize-1) data[i, ii] = row[ii].Substring(0, row[ii].Length - 1);
                else data[i, ii] = row[ii];
            }
        }
        return data;
    }
}
