using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace NaiveBayes
{
    class NBayes
    {
        // Following class was originally written in Java as a part of master's thesis. Later, code was "corrected" to 
        // work in C#. Next versions will be written in C#.

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

        public static void makeClassifier(String address) 
        {
        StreamReader fileReader = new StreamReader(address);
        attributes = fileReader.ReadLine().Split(splitRegex);
        horizontalLength = attributes.Length;
        attributesTypes = new String[horizontalLength];
        pointsOfDivision = new List<List<Double>>();
        attributesValuesNumbers = new List<int>();

        //Wprowadzanie typów atrybutów warunkowych.
        for(int i = 0; i<attributesTypes.Length; i++)
        {
            do
            {
                Console.WriteLine("Znaleziono atrybut: "+attributes[i] + " . Podaj jego typ: kategoryczny (wpisz \"K\"), liczbowy (wpisz \"L\"), pomiń w procesie (wpisz \"P\"). Dla atrybutu decyzyjnego wpisz \"D\".");
            attributesTypes[i]=Console.ReadLine();
            }while(!(attributesTypes[i].Equals("K") ^ attributesTypes[i].Equals("L") ^ attributesTypes[i].Equals("P") ^ attributesTypes[i].Equals("D")));

            if (attributesTypes[i].Equals("L"))
            {
                Console.WriteLine("Wartości atrybutu liczbowego zostaną podzielone na przedziały. Podaj w kolejności rosnącej punkty podziału, oddzielając je średnikiem. Np. \"1000.50;2000;3000\" zaowocuje utworzeniem przedziałów (...,1000.50), [1000.50,2000), [2000,3000),[3000,...). ");
                String[] sPoints = Console.ReadLine().Split(';');
                List<Double> dPoints = new List<Double>();
                foreach (String point in sPoints) {
                    try {
                        Double newDouble = Double.Parse(point);    
                        dPoints.Add(newDouble);
                    } catch (Exception ex) {
                        Console.WriteLine("Something is no yes with division points.");
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
                decAttPosition=i;
        }

            //==============================================
            // Prace remontowo-budowlane.

            // Liczenie ile jest rekordów.
            int verticalLength = 0;
            try
            {
                while (fileReader.Peek() > -1)
                {
                    fileReader.ReadLine();
                    verticalLength++;
                }
            }catch(Exception ex)
            {
                Console.WriteLine("Something is no yes with records counting...");
            }

            //Liczba rekordów jest mniejsza o 1 od liczby linii, ponieważ pierwsza linia zawiera nazwy atrybutów.
            int recordsNumber = verticalLength - 1;
            Console.WriteLine("W bazie danych jest "+ recordsNumber + " rekordów.");

            // "Powrót" do poczatkowej lini.
            fileReader = new StreamReader(address);

            //Tworzymy tablicę zawierającą wszystkie dane z bazy danych.
            String[,] dataBase = new String[recordsNumber,horizontalLength];
            String[] helpArray = new String[horizontalLength];
            fileReader.ReadLine();
            for (int i = 0; i<recordsNumber;i++)
            {
                helpArray=fileReader.ReadLine().Split(splitRegex);
                for(int ee=0;ee<helpArray.Length;ee++)
                {
                    dataBase[i, ee] = helpArray[ee];
                }
            }
            fileReader.Close();

        
        //==================================





        //Tworzymy listę z tablicami, które zawierają wszystkie występujące wartości atrybutów kategorycznych.
        attributesValues = new List<String[]>();

        for (int i = 0; i<horizontalLength;i++)
        {
            if (attributesTypes[i].Equals("K") || attributesTypes[i].Equals("D"))
            {
                HashSet<String> categoricalValuesSet = new HashSet<String>();
                /*foreach (String[] record in dataBase)
                {
                    categoricalValuesSet.Add(record[i]);
                }*/

                for (int tt = 0; tt < dataBase.GetLength(0); tt++)
                {
                    categoricalValuesSet.Add(dataBase[tt,i]);
                }

                    //String[] categoricalValues = categoricalValuesSet.ToArray(new String[0]);
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

        //Uzyskaliśmy już wszystkie niezbędne dane wymagane do utworzenia klasyfikatora.

           decisionAttributeValues = attributesValues.ElementAt(decAttPosition);
        double[,,] megaArray = new double[decisionAttributeValues.Length,horizontalLength,maxAttributesValuesNumber];

        double[] decisionAttributesCounters = new double[decisionAttributeValues.Length];

        //Zliczanie liczby wystąpień możliwych kombinacji.
        for(int n = 0; n<recordsNumber;n++)
        {
            for (int i = 0; i<decisionAttributeValues.Length; i++)
            {
                if (decisionAttributeValues[i].Equals(dataBase[n,decAttPosition]))
                {
                    for (int j = 0; j<horizontalLength; j++)
                    {
                        if (j!=decAttPosition)
                        {
                            //Dla atrybotow kategorycznych.
                            if (attributesTypes[j].Equals("K"))
                            {
                                for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++)
                                {
                                    if (attributesValues.ElementAt(j)[k].Equals(dataBase[n,j]))
                                        megaArray[i,j,k]++;
                                }
                            }
                            //Dla atrybutow liczbowych.
                            if (attributesTypes[j].Equals("L")) {
                                for (int k = pointsOfDivision.ElementAt(j).Count(); k > 0; k--)
                                {
                                    if (Double.Parse(dataBase[n,j]) >= pointsOfDivision.ElementAt(j).ElementAt(k - 1))
                                    {
                                        megaArray[i,j,k]++;
                                        break;
                                    }
                                    else if (k == 1)
                                    {
                                        megaArray[i,j,0]++;
                                    }
                                }
                            }
                        }
                    }
                    decisionAttributesCounters[i]++;
                }
            }
        }

        //Liczone są prawdopodobieństwa wystąpienia danej wartości atrybutu decyzyjnego.
        decisionAttributesProbabilities = new double[decisionAttributesCounters.Length];
        for (int i = 0; i<decisionAttributesCounters.Length;i++)
        {
            decisionAttributesProbabilities[i]=decisionAttributesCounters[i]/(double)recordsNumber;
        }

        //Tworzymy macierz prawdopodobieństw warunkowych.
        conditionalProbabilitiesMatrix = new double[decisionAttributeValues.Length,horizontalLength,maxAttributesValuesNumber];
            int zeroProblemCounter = 0;
        for (int i = 0; i<decisionAttributeValues.Length; i++)
        {
            for (int j = 0; j<horizontalLength; j++)
            {
                if(j!=decAttPosition && !(attributesTypes[j].Equals("P")))
                {
                        //Warunek rozwiązujący problem częstości zero.
                        //double[] zeroCheckArray = Array.CopyOf(megaArray[i,j,], attributesValuesNumbers.ElementAt(j));
                        double[] zeroCheckArray = new double[attributesValuesNumbers.ElementAt(j)];           
                        for (int rr=0;rr<attributesValuesNumbers.ElementAt(j);rr++)
                        {
                            zeroCheckArray[rr] = megaArray[i, j, rr];
                        }

                        Array.Sort(zeroCheckArray);   
                    if (zeroCheckArray[0]!=0)
                    {
                        for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++)
                        {
                            conditionalProbabilitiesMatrix[i,j,k] = megaArray[i,j,k] / decisionAttributesCounters[i];
                        }
                    }
                    else
                    {
                        //Estymator Laplace'a
                        for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++)
                        {
                            conditionalProbabilitiesMatrix[i,j,k] = (megaArray[i,j,k]+1)/(decisionAttributesCounters[i]+attributesValuesNumbers.ElementAt(j));
                                zeroProblemCounter++;
                        }
                    }
                }
            }
        }
            Console.WriteLine(zeroProblemCounter);

    }
//Koniec makeClassifier.




    public static void classifyRecords(String setName, String classifiedSetName, Boolean isTestSet) 
{
        StreamReader fileScanner=new StreamReader(setName);
        StreamWriter streamWriter = new StreamWriter(classifiedSetName, true);

        double recordNumber = 0;
        double correct = 0;
        double incorrect = 0;
        while(fileScanner.Peek()>-1)
        {
            recordNumber++;
            String line = fileScanner.ReadLine();
            String[] record = line.Split(splitRegex);
            double[] probabilities = (double[])decisionAttributesProbabilities.Clone();
                
            for(int i = 0; i<probabilities.Length;i++)
            {
                //Pętla po wszystkich atrybutach.
                for (int j = 0; j<horizontalLength; j++)
                {
                    if(j!=decAttPosition)
                    {
                        //Dla atrybutów kategorycznych.
                        if (attributesTypes[j].Equals("K")) {
                            for (int k = 0; k<attributesValuesNumbers.ElementAt(j); k++) {
                                if (attributesValues.ElementAt(j)[k].Equals(record[j]))
                                {
                                    probabilities[i] *= conditionalProbabilitiesMatrix[i,j,k];
                                }
                            }
                        }
                        //Dla atrybutów liczbowych.
                        if (attributesTypes[j].Equals("L")) {
                            for (int k = pointsOfDivision.ElementAt(j).Count(); k > 0; k--) {
                                if (Double.Parse(record[j]) >= pointsOfDivision.ElementAt(j).ElementAt(k - 1)) {
                                    probabilities[i] *= conditionalProbabilitiesMatrix[i,j,k];
                                    break;
                                } else if (k == 1) {
                                    probabilities[i] *= conditionalProbabilitiesMatrix[i,j,0];
                                }
                            }
                        }
                    }
                }
            }
            //Policzono prawdopodobieństwo przynależności rekordu do każdej z klas. Teraz trzeba znaleźć maksimum.
            int indexOfMaxProbability = 0;
            for(int i = 1; i<probabilities.Length;i++)
            {
                if (probabilities[i]>probabilities[indexOfMaxProbability])
                    indexOfMaxProbability=i;
            }
            Console.WriteLine("Rekord nr "+recordNumber+" został zaklasyfikowany do klasy "+ attributes[decAttPosition] + "="+decisionAttributeValues[indexOfMaxProbability]);

            if (isTestSet)
            {
                if (record[decAttPosition].Equals(decisionAttributeValues[indexOfMaxProbability]))
                    correct++;
                else
                    incorrect++;
            }

            //Wypisywanie kompletnego rekordu do pliku tekstowego.
            if(record.Length<horizontalLength)
            {
                List<String> completeRecord = new List<String>(record.ToList());
                completeRecord.Insert(decAttPosition,decisionAttributeValues[indexOfMaxProbability]);
                streamWriter.Write(String.Join(joinRegex,completeRecord));
            }
            else
            {
                record[decAttPosition]=decisionAttributeValues[indexOfMaxProbability];
                streamWriter.Write(String.Join(joinRegex,record));
            }
            //streamWriter.NewLine();
            streamWriter.WriteLine();
        }
        streamWriter.Close();

        if (isTestSet)
        {
            Console.WriteLine(correct+" rekordów zaklasyfikowano poprawnie.");
            Console.WriteLine(incorrect+" rekordów zaklasyfikowano błędnie.");
            Console.WriteLine("Trafność klasyfikacji wyniosła "+(100*correct/recordNumber)+"%.");
        }
    }

//Koniec classifyRecords


    
    //Koniec klasy NBayes.
    }
}
