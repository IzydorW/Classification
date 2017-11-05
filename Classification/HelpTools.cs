using System;
using System.IO;

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
    }
}