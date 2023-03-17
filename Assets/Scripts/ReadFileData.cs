using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReadFileData : MonoBehaviour
{

    //the plan: have the program look at every string. If it is new, then make it a new value starting at 0 and going up by 1.
    //for the array below, the first attribute represents the class, the second the row, and the third the value 
    //public float[] xVals;
    string myFilePath, fileName;
    //make an array for the max number of each attribute
    //make a number for the number of classes
    //make a number for the number of rows
    //static public float[,,] setosa;
    //static public float[,,] versicolor;
    //static public float[,,] virginica;

    static public float[,] setosa;
    static public float[,] versicolor;
    static public float[,] virginica;

    // Start is called before the first frame update
    void Awake()
    {
        fileName = "iris.data";
        myFilePath = Application.dataPath + "/FileData/" + fileName;
        ReadFromTheFile();
    }

    void ReadFromTheFile()
    {
        string[] Arrs = File.ReadAllLines(myFilePath);
        string[][] numberString = new string[Arrs.Length][];
        int setosaCount = 0;
        int versiCount = 0;
        int virgCount = 0;

        for (int i = 0; i < Arrs.Length; i++)
        {
            if (Arrs[i].Contains("setosa"))
            {
                setosaCount++;
            }
            else if (Arrs[i].Contains("versicolor"))
            {
                versiCount++;
            }
            else if (Arrs[i].Contains("virginica"))
            {
                virgCount++;
            }
        }

        setosa = new float[setosaCount,  4];
        versicolor = new float[versiCount, 4];
        virginica = new float[virgCount, 4];

        //Debug.Log(setosaCount);
        //Debug.Log(versiCount);
        //Debug.Log(virgCount);

        //looks through every line
        for (int i = 0; i < Arrs.Length; i++)
        {
            numberString[i] = Arrs[i].Split(',');
            //changes string to float values
            if (Arrs[i].Contains("setosa"))
            {
                setosa = seperateIntoIrisArrays(numberString, setosa, i, 0, "setosa");
            }
            else if (Arrs[i].Contains("versicolor"))
            {
                versicolor = seperateIntoIrisArrays(numberString, versicolor, i, setosaCount, "versicolor");
            }
            else if (Arrs[i].Contains("virginica"))
            {
                virginica = seperateIntoIrisArrays(numberString, virginica, i, versiCount + setosaCount, "virginica");
            }
        }

        /*for(int i = 0; i < setosa.GetLength(0); i++)
        {
            for(int j = 0; j < setosa.GetLength(1); j++)
            {
                Debug.Log(setosa[i, j]);
            }
        }*/
    }
    float[,] seperateIntoIrisArrays(string[][] unsep, float[,] irisType, int i, int cnt, string name)
    {
        for (int j = 0; j < unsep[i].Length; j++)
        {
            if (!unsep[i][j].Contains("-"))
            {
                irisType[i - cnt,j] = float.Parse(unsep[i][j]);
                //Debug.Log(name + " " + irisType[i - cnt, j]);
            }
            else
            {
                //Debug.Log("skip");
            }
        }
        return irisType;
    }
}
