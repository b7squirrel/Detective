using UnityEngine;
using System;

public class ReadData
{
    string[,] data;
    int lineSize, rowSize;
    
    public string[,] GetText(TextAsset txt)
    {
        try
        {
            // 텍스트 정규화: 모든 줄바꿈을 \n으로 통일
            string currentText = txt.text
                .Replace("\r\n", "\n")  // Windows 줄바꿈 → Unix
                .Replace("\r", "\n");   // Old Mac 줄바꿈 → Unix
            
            // 마지막 문자가 줄바꿈이면 제거
            if (currentText.EndsWith("\n"))
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
            }
            
            // 빈 줄 제거하고 분할
            string[] lines = currentText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length == 0)
            {
                Debug.LogError("데이터 파일이 비어있습니다.");
                return new string[0, 0];
            }
            
            lineSize = lines.Length;
            
            // 첫 번째 줄에서 열 크기 결정
            string[] firstRowData = lines[0].Split('\t');
            rowSize = firstRowData.Length;
            
            data = new string[lineSize, rowSize];
            
            for (int i = 0; i < lineSize; i++)
            {
                string[] rowData = lines[i].Split('\t');
                
                // 행의 열 개수가 다를 경우 처리
                int actualRowSize = Mathf.Min(rowData.Length, rowSize);
                
                for (int j = 0; j < actualRowSize; j++)
                {
                    // 각 셀의 데이터 정리 (앞뒤 공백, 특수문자 제거)
                    string cellData = rowData[j].Trim();
                    
                    // 마지막 문자가 캐리지 리턴이면 제거
                    if (cellData.EndsWith("\r"))
                    {
                        cellData = cellData.Substring(0, cellData.Length - 1);
                    }
                    
                    data[i, j] = cellData;
                }
                
                // 부족한 열은 빈 문자열로 채움
                for (int j = actualRowSize; j < rowSize; j++)
                {
                    data[i, j] = "";
                }
            }
            
            Debug.Log($"데이터 로드 완료: {lineSize}행 x {rowSize}열");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 읽기 오류: {e.Message}");
            return new string[0, 0];
        }
    }
    
    // 디버깅용 메서드 (선택사항)
    public void PrintData()
    {
        if (data == null) return;
        
        for (int i = 0; i < lineSize; i++)
        {
            string row = "";
            for (int j = 0; j < rowSize; j++)
            {
                row += $"[{data[i, j]}]\t";
            }
            Debug.Log($"Row {i}: {row}");
        }
    }
}