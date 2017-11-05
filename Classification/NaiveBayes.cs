using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Classification
{
    class NaiveBayes
    {
        // Following class was originally written in Java as a part of master's thesis. Later, due to some similarity of
        // Java and C#, code was "corrected" to work in C#. Next versions will be written in C#.

        private static String[] attributes;
        private static String[] attributesTypes;
        private static String[] decisionAttributeValues;
        private static List<String[]> attributesValues;
        private static List<List<Double>> pointsOfDivision;
        private static double[] decisionAttributesProbabilities;
        private static double[,,] conditionalProbabilitiesMatrix;
        private static char splitRegex = ',';
        private static String joinRegex = ",";
        private static int decAttPosition;
        private static List<int> attributesValuesNumbers;
        private static int horizontalLength;

        public static void MakeClassifier(String addressOfTrainingSet) 
        {
            StreamReader fileReader = new StreamReader(addressOfTrainingSet);
            attributes = fileReader.ReadLine().Split(splitRegex);
            horizontalLength = attributes.Length;
            attributesTypes = new String[horizontalLength];
            pointsOfDivision = new List<List<Double>>();
            attributesValuesNumbers = new List<int>();
        
            // TODO: change marks of attributes types for english equivalents.
            // User enters types of attributes.
            for(int i = 0; i < attributesTypes.Length; i++)
            {
                do
                {
                    Console.WriteLine($"Following attribute has been founded: {attributes[i]}. Enter its type: \"K\" " +
                                      $"for categorical, \"L\" for number valued or \"D\" for decision attribute." +
                                      $"Type \"P\" to skip it in process.");
                    attributesTypes[i]=Console.ReadLine();
                } while(!(attributesTypes[i].Equals("K") ^ attributesTypes[i].Equals("L") ^ 
                          attributesTypes[i].Equals("P") ^ attributesTypes[i].Equals("D")));
        
                if (attributesTypes[i].Equals("L"))
                {
                    Console.WriteLine("Values of attribute will be divided into right-closed intervals. Enter division " +
                                      "points in ascending order, separating them with \";\".");
                    String[] sPoints = Console.ReadLine().Split(';');
                    List<Double> dPoints = new List<Double>();
                    foreach (String point in sPoints) 
                    {
                        try 
                        {                            
                            dPoints.Add(Double.Parse(point));
                        } 
                        catch (FormatException ex) 
                        {
                            Console.WriteLine("Something is wrong with division points.");
                        }
                    }
                    
                    attributesValuesNumbers.Add(dPoints.Count()+1);
                    pointsOfDivision.Add(dPoints);
                }
                else
                {
                    pointsOfDivision.Add(new List<Double>());
                    attributesValuesNumbers.Add(0);
                }
                
                if (attributesTypes[i].Equals("D"))
                {
                    decAttPosition=i;
                }
            }
        
            // Records counting.
            int verticalLength = 0;
            try
            {
                while (fileReader.Peek() > -1)
                {
                    fileReader.ReadLine();
                    verticalLength++;
                }
            }
            catch(IOException ex)
            {
                Console.WriteLine("Something is wrong with records counting...");
            }
            
            // TODO: separate case when first line isn't attributes names line. 
            // Records number is 1 less than lines number, because first line contains attributes names.
            int recordsNumber = verticalLength - 1;
            Console.WriteLine("W bazie danych jest "+ recordsNumber + " rekordów.");
    
            // Pointer back to first line.
            fileReader = new StreamReader(addressOfTrainingSet);
    
            // Create array containing all data from database.
            String[,] dataBase = new String[recordsNumber,horizontalLength];
            String[] helpArray = new String[horizontalLength];
            fileReader.ReadLine();
            for (int i = 0; i< recordsNumber; i++)
            {
                helpArray=fileReader.ReadLine().Split(splitRegex);
                for(int q = 0; q < helpArray.Length; q++)
                {
                    dataBase[i,q] = helpArray[q];
                }
            }
            
            fileReader.Close();
        
            // Create lists of arrays, containing all occurring categorical attributes values.
            attributesValues = new List<String[]>();
        
            for (int i = 0; i <  horizontalLength; i++)
            {
                if (attributesTypes[i].Equals("K") || attributesTypes[i].Equals("D"))
                {
                    HashSet<String> categoricalValuesSet = new HashSet<String>();
        
                    for (int tt = 0; tt < dataBase.GetLength(0); tt++)
                    {
                        categoricalValuesSet.Add(dataBase[tt,i]);
                    }
        
                    String[] categoricalValues = new String[categoricalValuesSet.Count];
                    categoricalValuesSet.CopyTo(categoricalValues);
                    attributesValues.Add(categoricalValues);
                    attributesValuesNumbers.Insert(i,categoricalValues.Length);
                }
                else
                {
                    attributesValues.Add(new String[0]);
                }
            }
            
            int maxAttributesValuesNumber = attributesValuesNumbers.Max();
        
            // At this point, all needed data to make classifier was gathered.
            decisionAttributeValues = attributesValues.ElementAt(decAttPosition);
            double[,,] countMatrix = new double[decisionAttributeValues.Length,horizontalLength,
                maxAttributesValuesNumber];
            double[] decisionAttributesCounters = new double[decisionAttributeValues.Length];
        
            // Counting occurrences of possible combinations.
            for(int n = 0; n < recordsNumber; n++)
            {
                for (int i = 0; i < decisionAttributeValues.Length; i++)
                {
                    if (decisionAttributeValues[i].Equals(dataBase[n,decAttPosition]))
                    {
                        for (int j = 0; j < horizontalLength; j++)
                        {
                            if (j!=decAttPosition)
                            {
                                // For categorical attributes.
                                if (attributesTypes[j].Equals("K"))
                                {
                                    for (int k = 0; k< attributesValuesNumbers.ElementAt(j); k++)
                                    {
                                        if (attributesValues.ElementAt(j)[k].Equals(dataBase[n,j]))
                                            countMatrix[i,j,k]++;
                                    }
                                }
                                
                                // For number valued attributes.
                                if (attributesTypes[j].Equals("L")) {
                                    for (int k = pointsOfDivision.ElementAt(j).Count; k > 0; k--)
                                    {
                                        if (Double.Parse(dataBase[n,j]) >= 
                                            pointsOfDivision.ElementAt(j).ElementAt(k - 1))
                                        {
                                            countMatrix[i,j,k]++;
                                            break;
                                        }
                                        
                                        if (k == 1)
                                        {
                                            countMatrix[i,j,0]++;
                                        }
                                    }
                                }
                            }
                        }
                        
                        decisionAttributesCounters[i]++;
                    }
                }
            }
        
            // Counts probabilities of occurrences decision attributes values.
            decisionAttributesProbabilities = new double[decisionAttributesCounters.Length];
            for (int i = 0; i< decisionAttributesCounters.Length; i++)
            {
                decisionAttributesProbabilities[i]=decisionAttributesCounters[i] / recordsNumber;
            }
        
            // Create matrix of conditional probabilities.
            conditionalProbabilitiesMatrix = new double[decisionAttributeValues.Length,horizontalLength,
                maxAttributesValuesNumber];
            
            for (int i = 0; i<decisionAttributeValues.Length; i++)
            {
                for (int j = 0; j<horizontalLength; j++)
                {
                    if(j!=decAttPosition && !(attributesTypes[j].Equals("P")))
                    {
                        double[] zeroCheckArray = new double[attributesValuesNumbers.ElementAt(j)];           
                        for (int r = 0; r < attributesValuesNumbers.ElementAt(j); r++)
                        {
                            zeroCheckArray[r] = countMatrix[i,j,r];
                        }
                        Array.Sort(zeroCheckArray);   

                        // Solves zero problem.
                        if (zeroCheckArray[0]!=0)
                        {
                            for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++)
                            {
                                conditionalProbabilitiesMatrix[i,j,k] = countMatrix[i,j,k] / 
                                                                        decisionAttributesCounters[i];
                            }
                        }
                        else
                        {
                            // Use Laplacian estimator.
                            for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++)
                            {
                                conditionalProbabilitiesMatrix[i,j,k] = (countMatrix[i,j,k]+1) /
                                    (decisionAttributesCounters[i]+attributesValuesNumbers.ElementAt(j));
                            }
                        }
                    }
                }
            }
        }


        public static void ClassifyRecords(String addressOfSetToClassify, String addressToSaveClassifiedSet, 
            Boolean isTestSet) 
        {
            StreamReader fileReader = new StreamReader(addressOfSetToClassify);
            StreamWriter fileWriter = new StreamWriter(addressToSaveClassifiedSet, true);
    
            double recordNumber = 0;
            double correct = 0;
            double incorrect = 0;
        
            while(fileReader.Peek() > -1)
            {
                recordNumber++;
                String line = fileReader.ReadLine();
                String[] record = line.Split(splitRegex);
                double[] probabilities = (double[])decisionAttributesProbabilities.Clone();
                    
                for(int i = 0; i< probabilities.Length; i++)
                {
                    for (int j = 0; j < horizontalLength; j++)
                    {
                        if(j != decAttPosition)
                        {
                            // For categorical attributes.
                            if (attributesTypes[j].Equals("K")) {
                                for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++) 
                                {
                                    if (attributesValues.ElementAt(j)[k].Equals(record[j]))
                                    {
                                        probabilities[i] *= conditionalProbabilitiesMatrix[i,j,k];
                                    }
                                }
                            }
                            
                            // For number valued attributes.
                            if (attributesTypes[j].Equals("L")) 
                            {
                                for (int k = pointsOfDivision.ElementAt(j).Count; k > 0; k--) {
                                    if (Double.Parse(record[j]) >= pointsOfDivision.ElementAt(j).ElementAt(k - 1)) 
                                    {
                                        probabilities[i] *= conditionalProbabilitiesMatrix[i,j,k];
                                        break;
                                    }
                                    
                                    if (k == 1) {
                                        probabilities[i] *= conditionalProbabilitiesMatrix[i,j,0];
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Probabilities of new record probability of membership of each decision class was computed. Now aim is
                // to find maximum of them.
                int indexOfMaxProbability = 0;
                for(int i = 1; i < probabilities.Length; i++)
                {
                    if (probabilities[i] > probabilities[indexOfMaxProbability])
                        indexOfMaxProbability=i;
                }
    
                Console.WriteLine($"Record {recordNumber} has been classified into decision class " +
                                  $"{attributes[decAttPosition]}={decisionAttributeValues[indexOfMaxProbability]}.");
    
                if (isTestSet)
                {
                    if (record[decAttPosition].Equals(decisionAttributeValues[indexOfMaxProbability]))
                    {
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }
    
                // Writing complete record to text file.
                if(record.Length < horizontalLength)
                {
                    List<String> completeRecord = new List<String>(record.ToList());
                    completeRecord.Insert(decAttPosition,decisionAttributeValues[indexOfMaxProbability]);
                    fileWriter.Write(String.Join(joinRegex,completeRecord));
                }
                else
                {
                    record[decAttPosition]=decisionAttributeValues[indexOfMaxProbability];
                    fileWriter.Write(String.Join(joinRegex,record));
                }
                
                fileWriter.WriteLine();
            }
        
            fileWriter.Close();
    
            if (isTestSet)
            {
                Console.WriteLine($"{correct} records were classified correctly.");
                Console.WriteLine($"{incorrect} records were classified incorrectly.");
                Console.WriteLine($"Accuracy of classification was: {correct/recordNumber:P}.");
            }
        }
    }
}
