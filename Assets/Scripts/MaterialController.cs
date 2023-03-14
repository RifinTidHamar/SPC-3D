using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Handles all Visuals derived from the two vectors, X1 and A. In all honesty this could be split up for organization sake. Ill leave that up to future programmers
public class MaterialController : MonoBehaviour
{
    private const int NUM_PAIR_COORD = 2; //The number of subcoordinates
    private const float VALUE_TO_CUBE_FRAC_SCALE = 5f / 12f; //cubeLengthWidth (always 5 unless scaled in editor) / maximum point height
    private const float X_SCALE = 5;
    private const float Y_SCALE = 5;
    public GameObject prefabPointFirstSubCoord;
    public GameObject prefabPointSecondSubCoord;
    public GameObject prefabVector;
    public GameObject firstSubCoord;
    public GameObject secondSubCoord;
    public Material rulePlaneC1;
    public Material rulePlaneC2;

    GameObject[] coordPointArr;// = new GameObject[100];
    //public Material pointMat;
    GameObject[] coordPlaneArr;// = new GameObject[100];
    GameObject[] vectorArr;
    //public Text[] restOfFuncTexts;
    public Material[] HeightPlanes;
    //public Text fX;

    private Color class2 = new Color(255, 0, 186, 255);
    private Color class1 = new Color(255, 255, 0, 255);
    private Color grey = new Color(0.2f, 0.2f, 0.2f, 0);
    private float[] fxi = new float[50];//f(x) input
    private float[,] xVals = new float [50,4];
    private float[,] normXVals = new float[50,4];
    private float[] aVals = new float[4];
    private float[] ruleC1 = { 1, 0, 1, 0 };
    private float[] ruleC2 = { 1, 0, 1, 0 };
    private float c = 0.5f;
    private float maxHeight = 12;
    private float maxX = 4.5f;
    private float maxY = 6;
    //private double[] restOfFunction = new double[3]; //ie f12(x) = a1x1 + a2x2 + restOfFunction - f(x)
    private float[,] pointHeight = new float[3, 4];
    [Range(0.1f, 10f)]
    public float xVal;
    [Range(0.1f, 10f)]
    public float yVal;
    void Start()
    {

        xVals = ReadFileData.setosa;
        //Debug.Log(xVals.GetLength(0));
        coordPointArr = new GameObject[xVals.GetLength(0)* 2];// multiplied by two because it there are two subcoordinates for every 4 values (which makes up one xVal)
        coordPlaneArr = new GameObject[xVals.GetLength(0) * 2];
        vectorArr = new GameObject[xVals.GetLength(0)];
        for (int i = 0; i < xVals.GetLength(0); i++)
        {
            //the two points: one in 1st sub coordinate, and one in the second
            GameObject point1 = Instantiate(prefabPointFirstSubCoord, firstSubCoord.transform);
            GameObject point2 = Instantiate(prefabPointSecondSubCoord, secondSubCoord.transform);
            //GameObject vector = Instantiate(prefabVector);

            //setting the two points up
            point1.transform.localPosition = new Vector3(0, 0, 0);
            point2.transform.localPosition = new Vector3(0, 0, 0);
            coordPointArr[i * 2] = point1.transform.GetChild(1).gameObject;
            coordPointArr[i * 2 + 1] = point2.transform.GetChild(1).gameObject;

            Renderer renderer = point1.transform.GetChild(0).gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/coordinateLines"));
            renderer.material.renderQueue = 3000;
            renderer.material.SetFloat("_Transparency", 0.01f);

            //setting the two coordinate lines up
            renderer = point2.transform.GetChild(0).gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/coordinateLines"));
            renderer.material.renderQueue = 3000;
            renderer.material.SetFloat("_Transparency", 0.01f);
            coordPlaneArr[i * 2] = point1.transform.GetChild(0).gameObject;
            coordPlaneArr[i * 2 + 1] = point2.transform.GetChild(0).gameObject;

            ////setting the vector between two points up
            //renderer = vector.gameObject.GetComponent<Renderer>();
            //renderer.material = new Material(Shader.Find("Custom/vector"));
            //renderer.material.SetColor("_Color", new Vector4(0.7490196f, 0, 0.7490196f, 1));
            //vectorArr[i] = vector;

        }

        /*for (int i = 0; i < normXVals.GetLength(0); i++)
        {
            for (int j = 0; j < normXVals.GetLength(1); j++)
            {
                Debug.Log(normXVals[i, j]);
            }
        }*/
    }
    //called once per frame
    private void Update()
    {
        rulePlaneC1.SetFloat("_TX", ruleC1[0]);
        rulePlaneC1.SetFloat("_BX", ruleC1[1]);
        rulePlaneC1.SetFloat("_TY", ruleC1[2]);
        rulePlaneC1.SetFloat("_BY", ruleC1[3]);

        rulePlaneC2.SetFloat("_TX", ruleC2[0]);
        rulePlaneC2.SetFloat("_BX", ruleC2[1]);
        rulePlaneC2.SetFloat("_TY", ruleC2[2]);
        rulePlaneC2.SetFloat("_BY", ruleC2[3]);

        setHeightPlaneCorners();
        int n = 0;
        normXVals = normalize(xVals);
        for (int j = 0; j < xVals.GetLength(0); j++)
        {
            fxi[j] = setFX(aVals, xVals, j);
            //Material vec = vectorArr[j].GetComponent<Renderer>().material;
            bool skipNextMat = false;
            bool outsideRules = false;
            //points
            for (int i = 0; i < NUM_PAIR_COORD; i++)
            {
                coordPointArr[n + i].transform.localPosition = new Vector3((float)normXVals[j, i * 2] * X_SCALE - 2.5f, (float)normXVals[j, i * 2 + 1] * Y_SCALE - 2.836596f, (float)(fxi[j] * VALUE_TO_CUBE_FRAC_SCALE) - 2.5f);
                Vector3 currentCoord = coordPointArr[n + i].transform.localPosition;

                int otherPointIndex;
                if (i % 2 == 0)
                {
                    //vec.SetVector("_Pos1", coordPointArr[n + i].transform.position);
                    otherPointIndex = 1;
                }
                else
                {
                    //vec.SetVector("_Pos2", coordPointArr[n + i].transform.position);
                    otherPointIndex = -1;
                }
                GameObject[] connectedPoints = { coordPointArr[n + i + otherPointIndex], coordPointArr[n + i] };
                Material point = coordPointArr[n + i].GetComponent<Renderer>().material;
                Material otherPoint = coordPointArr[n + i + otherPointIndex].GetComponent<Renderer>().material;
                Material coordLines = coordPlaneArr[n + i].GetComponent<Renderer>().material;
                Material otherCoordLine = coordPlaneArr[n + i + otherPointIndex].GetComponent<Renderer>().material;
                setCoordPlane(coordLines, currentCoord, (float)(c * maxHeight) * VALUE_TO_CUBE_FRAC_SCALE - 2.5f);
                bool ruleCoordOne = (ruleC1[0] >= (float)normXVals[j, i * 2] && (float)normXVals[j, i * 2] >= ruleC1[1] &&
                        ruleC1[2] >= (float)normXVals[j, i * 2 + 1] && (float)normXVals[j, i * 2 + 1] >= ruleC1[3]) && i % 2 == 1;
                bool ruleCoordTwo = (ruleC2[0] >= (float)normXVals[j, i * 2] && (float)normXVals[j, i * 2] >= ruleC2[1] &&
                        ruleC2[2] >= (float)normXVals[j, i * 2 + 1] && (float)normXVals[j, i * 2 + 1] >= ruleC2[3]) && i % 2 == 0;
                if (!skipNextMat)
                { 
                    if (ruleCoordOne || ruleCoordTwo)
                    {
                        coordLines.SetInt("faded", 0);
                        if (fxi[j] <= (c * maxHeight))
                        {
                            point.SetColor("_Color", class2);//class 2
                            //vec.SetColor("_Color", class2);
                        }
                        else
                        {
                            point.SetColor("_Color", class1); //class 1
                            //vec.SetColor("_Color", class1);
                        }
                        for (int p = 0; p < connectedPoints.Length; p++)
                        {
                            connectedPoints[p].transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                        }
                    }
                    else
                    {
                        coordLines.SetInt("faded", 1);
                        otherCoordLine.SetInt("faded", 1);
                        point.SetColor("_Color", grey);
                        otherPoint.SetColor("_Color", grey);
                        for (int p = 0; p < connectedPoints.Length; p++)
                        {
                            connectedPoints[p].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        }
                        outsideRules = true;
                        skipNextMat = true;
                    }
                }
            }
            if(outsideRules)
            {
                //vectorArr[j].GetComponent<Renderer>().enabled = false;
            }
            else
            {
                //vectorArr[j].GetComponent<Renderer>().enabled = true;
            }
            n += 2;
            //draws the connecting line for points
            //vector[0].SetVector("_Pos1", coordPoint[0].transform.position);
            //vector[0].SetVector("_Pos2", coordPoint[1].transform.position);
            //vector[0].SetVector("_Pos3", coordPoint[2].transform.position);
        }
    }
    //calcultes f(x) based off "A" vector and "X" vector
    float setFX(float[] a, float[,] x, int i)
    {
        float f_x = a[0] * x[i,0] + a[1] * x[i,1] + a[2] * x[i,2] + a[3] * x[i,3];
        return f_x;
    }

    //finds the rest of the height function to set f(x) to zero. For example, f_12(X) = a1x1 + a2x2 + restOfVectors - f(x)= 0. This finds restOfVectors for f_12(x), f_34(x), f_56(x). 
    /*double[] setRestOfFunctions(double[] a, double[] x)
    {
        double[] rof = new double[3];//rest of function
        rof[0] = (a[2] * x[2] + a[3] * x[3] + a[4] * x[4] + a[5] * x[5]); //f12
        rof[1] = (a[0] * x[0] + a[1] * x[1] + a[4] * x[4] + a[5] * x[5]); //f34
        rof[2] = (a[0] * x[0] + a[1] * x[1] + a[2] * x[2] + a[3] * x[3]); //f56
        return rof;
    }*/
    //sets each corner of the yellow height planes at position (0,0), (0,1) (1,1), (1,0) for each subcoordinate
    void setHeightPlaneCorners()
    {
        for (int i = 0; i < NUM_PAIR_COORD; i++)
        {
            float newC = (float)-(c * maxHeight) * VALUE_TO_CUBE_FRAC_SCALE; //for fitting purposes into the cubes of the subcoordinates. Otherwise it does not match Fx in heigh
            pointHeight[i, 0] = newC;// setPointHeight(0, 0, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(0,0)
            pointHeight[i, 1] = newC;// setPointHeight(1, 0, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(1,0)
            pointHeight[i, 2] = newC;// setPointHeight(1, 1, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(1,1)
            pointHeight[i, 3] = newC;// setPointHeight(0, 1, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(0,1);
            HeightPlanes[i].SetFloat("_Pos0", (float)(pointHeight[i, 0]));
            HeightPlanes[i].SetFloat("_Pos1", (float)(pointHeight[i, 1]));
            HeightPlanes[i].SetFloat("_Pos2", (float)(pointHeight[i, 2]));
            HeightPlanes[i].SetFloat("_Pos3", (float)(pointHeight[i, 3]));
        }
    }

    //sets the height of a corner for the height plane.
    /*double setPointHeight(double x1, double x2, double a1, double a2, double rof)
    {
        double height = (x1 * a1 + x2 * a2 + rof - fxi) * 2.5f;// * 5;
        return height;
    }*/

    //sets the values for coordPlane shader
    void setCoordPlane(Material lines, Vector3 pos, float C)
    {
        //The values, -2.5, and -2.84 properly set the lines within their subcoordinate
        //The addition/subtraction of 0.01 just allows for some offset of the coordlines so that they don't z-fight with other lines.
        lines.SetVector("_Pos0", new Vector3(-2.5f, -2.846596f, -2.5f + 0.01f));
        lines.SetVector("_Pos1", new Vector3(pos.x - 0.01f, -2.846596f, -2.5f + 0.01f));
        lines.SetVector("_Pos2", new Vector3(pos.x - 0.01f, pos.y, -2.5f + 0.01f));
        lines.SetVector("_Pos3", new Vector3(pos.x, pos.y + 0.01f, pos.z));
        lines.SetVector("_C", new Vector3(pos.x, pos.y + 0.01f, C));
    }

    float[,] normalize(float[,] x)
    {
        float[,] normArr = new float[x.GetLength(0), x.GetLength(1)];

        /*float xScale = 6;
        float yScale = 4.5f;*/

        for(int i = 0; i < normArr.GetLength(0); i++)
        {
            for (int j = 0; j < normArr.GetLength(1); j++)
            {
                if(j % 2 == 0)
                    normArr[i,j] = x[i,j] / xVal;
                else
                    normArr[i,j] = x[i, j] / yVal;
            }
        }
        
        return normArr;
    }
   
    //These set of functions read in the A vector from the UI, one element at a time
    public void readA0(string aInp)
    {
        if (float.TryParse(aInp, out _))
            aVals[0] = float.Parse(aInp);
    }
    public void readA1(string aInp)
    {
        if (float.TryParse(aInp, out _))
            aVals[1] = float.Parse(aInp);
    }
    public void readA2(string aInp)
    {
        if (float.TryParse(aInp, out _))
            aVals[2] = float.Parse(aInp);
    }
    public void readA3(string aInp)
    {
        if (float.TryParse(aInp, out _))
            aVals[3] = float.Parse(aInp);
    }

    //reads in the C input
    public void readC(string cInp)
    {
        if (float.TryParse(cInp, out _))
            c = float.Parse(cInp);
    }

    //reads the rules for coord 1
    public void readX1C1(string xInp)
    {
        if (float.TryParse(xInp, out _))
            ruleC1[0] = float.Parse(xInp);
    }
    public void readX2C1(string xInp)
    {
        if (float.TryParse(xInp, out _))
            ruleC1[1] = float.Parse(xInp);
    }
    public void readY1C1(string yInp)
    {
        if (float.TryParse(yInp, out _))
            ruleC1[2] = float.Parse(yInp);
    }
    public void readY2C1(string yInp)
    {
        if (float.TryParse(yInp, out _))
            ruleC1[3] = float.Parse(yInp);
    }

    //reads the rules for coord2
    public void readX1C2(string xInp)
    {
        if (float.TryParse(xInp, out _))
            ruleC2[0] = float.Parse(xInp);
    }
    public void readX2C2(string xInp)
    {
        if (float.TryParse(xInp, out _))
            ruleC2[1] = float.Parse(xInp);
    }
    public void readY1C2(string yInp)
    {
        if (float.TryParse(yInp, out _))
            ruleC2[2] = float.Parse(yInp);
    }
    public void readY2C2(string yInp)
    {
        if (float.TryParse(yInp, out _))
            ruleC2[3] = float.Parse(yInp);
    }

    /*//These set of functions read in the X vector from the UI, one element at a time
    public void readX(string xInp)
    {
       xVals[0] = double.Parse(xInp);
    }
    public void readY(string yInp)
    {
        xVals[1] = double.Parse(yInp);
    }
    public void readZ(string zInp)
    {
        xVals[2] = double.Parse(zInp);
    }
    public void readW(string wInp)
    {
        xVals[3] = double.Parse(wInp);
    }
    public void readV(string vInp)
    {
        xVals[4] = double.Parse(vInp);
    }
    public void readU(string uInp)
    {
        xVals[5] = double.Parse(uInp);
    }*/
}
