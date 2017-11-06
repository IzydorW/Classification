using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Classification
{
    public class HelpTools
    {
        // TODO: add to SplitSet possibility to choose ratio of records added to training/test sets.
        // TODO: make version of SplitSet that allows to split original set into more than two sets.
        
        public static void SplitSet(String addressOfStartSet, String addressToSaveTrainingSet, 
            String addressToSaveTestSet) 
        {
            StreamReader fileReader = new StreamReader(addressOfStartSet);

            StreamWriter trainingSetWriter = new StreamWriter(addressToSaveTrainingSet, true);
            StreamWriter testSetWriter = new StreamWriter(addressToSaveTestSet, true);
            int lineNumber=0;
            int trainingRecords=0;
            int testRecords=0;
            
            while(fileReader.Peek() > -1)
            {
                String line = fileReader.ReadLine();
                if (lineNumber%3==0)
                {
                    testSetWriter.WriteLine(line);
                    testRecords++;
                }
                else
                {
                    trainingSetWriter.WriteLine(line);
                    trainingRecords++;
                }
                
                lineNumber++;
            }
            
            trainingSetWriter.Close();
            testSetWriter.Close();
            Console.WriteLine($"Number of training records: {trainingRecords}.");
            Console.WriteLine($"Number of test records: {testRecords}.");
        }

        public static int CountLines(String fileAddress)
        {
            StreamReader fileReader = new StreamReader(fileAddress);
            int linesNumber = 0;
            
            try
            {
                while (fileReader.Peek() > -1)
                {
                    fileReader.ReadLine();
                    linesNumber++;
                }
            }
            catch(IOException ex)
            {
                Console.WriteLine("Something is wrong with lines counting...");
            }
            
            fileReader.Close();
            return linesNumber;
        }

        public static String[,] DataBaseToArray(String fileAddress, String dataSeparator, Boolean withoutFirstLine)
        {
            StreamReader fileReader = new StreamReader(fileAddress);
            String[] splitRegex = {dataSeparator};
            int verticalLength = CountLines(fileAddress);
            int horizontalLength = 0;
            String[] lineArray;
            
            try
            {
                lineArray = fileReader.ReadLine().Split(splitRegex, StringSplitOptions.None);
                horizontalLength = lineArray.Length;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Something is wrong with reading first line.");
            }

            if (!withoutFirstLine)
            {
                fileReader = new StreamReader(fileAddress);
            }
            
            String[,] dataBase = new String[verticalLength,horizontalLength];
            try
            {
                for (int i = 0; i < verticalLength; i++)
                {
                    lineArray = fileReader.ReadLine().Split(splitRegex, StringSplitOptions.None);
                    for (int j = 0; j < lineArray.Length; j++)
                    {
                        dataBase[i, j] = lineArray[j];
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            fileReader.Close();
            return dataBase;
        }

        public static void ArrayToDataBase(String[,] array, String fileAddress, String dataSeparator)
        {
            StreamWriter fileWriter = new StreamWriter(fileAddress);
            String[] lineArray = new String[array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    lineArray[j] = array[i, j];
                }
                fileWriter.WriteLine(String.Join(dataSeparator, lineArray));
            }
            
            fileWriter.Close();
        }
        
    }
}