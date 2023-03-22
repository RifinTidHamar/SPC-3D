using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Handles all Visuals derived from the two vectors, X1 and A. In all honesty this could be split up for organization sake. Ill leave that up to future programmers
public class MaterialController : MonoBehaviour
{
    private const int NUM_PAIR_COORD = 2; //The number of subcoordinates
    private float high_fx = 0;
    private float VALUE_TO_CUBE_FRAC_SCALE; //cubeLengthWidth (always 5 unless scaled in editor) / maximum point height
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
    GameObject[] attribContribPointArr;
    //public Material pointMat;
    GameObject[] coordPlaneArr;// = new GameObject[100];
    GameObject[] vectorArr;
    //public Text[] restOfFuncTexts;
    public Material[] HeightPlanes;
    //public Text fX;

    private Color[] classColors = { new Color(255, 0, 0, 255), new Color(0, 255, 0, 255), new Color(0, 0, 255, 255)};
    private Color class2 = new Color(255, 0, 186, 255);
    private Color class1 = new Color(255, 255, 0, 255);
    private Color grey = new Color(0.2f, 0.2f, 0.2f, 0);
    private float[] fxi;// = new float[50];//f(x) input
    private float[][] fxiParts;// = new float[50][];
    private float[][][] data; //= new float [50,4];
    //data normalized by column
    private float[][][] normPerAttribData;// = new float[50,4];
    private float[] aVals = { 1, 1, 1, 1 };
    private float[] ruleC1 = { 1, 0, 1, 0 };
    private float[] ruleC2 = { 1, 0, 1, 0 };
    private float c = 0.5f;
    //private double[] restOfFunction = new double[3]; //ie f12(x) = a1x1 + a2x2 + restOfFunction - f(x)
    private float[,] pointHeight = new float[3, 4];

    float xShift = 2.5f;
    float yShift = 2.836596f;
    float zShift = 2.5f;// 0.008552f;
    void Start()
    {
        //intitialize data
        data = ReadFileData.data;
        fxi = new float[ReadFileData.setCount];
        fxiParts = new float[ReadFileData.setCount][];
        int setCount = ReadFileData.setCount;

        //Debug.Log(xVals.GetLength(0));
        coordPointArr = new GameObject[setCount * 2];// multiplied by two because it there are two subcoordinates for every 4 values (which makes up one xVal)
        coordPlaneArr = new GameObject[setCount * 2];
        attribContribPointArr = new GameObject[setCount * 2];
        vectorArr = new GameObject[setCount];
        for (int i = 0; i < setCount; i++)
        {
            //the two points: one in 1st sub coordinate, and one in the second
            GameObject point1 = Instantiate(prefabPointFirstSubCoord, firstSubCoord.transform);
            GameObject point2 = Instantiate(prefabPointSecondSubCoord, secondSubCoord.transform);
            GameObject vector = Instantiate(prefabVector);

            //setting the two points up
            point1.transform.localPosition = new Vector3(0, 0, 0);
            point2.transform.localPosition = new Vector3(0, 0, 0);
            coordPointArr[i * 2] = point1.transform.GetChild(1).gameObject;
            coordPointArr[i * 2 + 1] = point2.transform.GetChild(1).gameObject;

            attribContribPointArr[i * 2] = point1.transform.GetChild(2).gameObject;
            attribContribPointArr[i * 2 + 1] = point2.transform.GetChild(2).gameObject;

            Renderer renderer = point1.transform.GetChild(0).gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/coordinateLines"));
            renderer.material.renderQueue = 3000;
            renderer.material.SetFloat("_Transparency", 0.002f);

            //setting the two coordinate lines up
            renderer = point2.transform.GetChild(0).gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/coordinateLines"));
            renderer.material.renderQueue = 3000;
            renderer.material.SetFloat("_Transparency", 0.002f);
            coordPlaneArr[i * 2] = point1.transform.GetChild(0).gameObject;
            coordPlaneArr[i * 2 + 1] = point2.transform.GetChild(0).gameObject;

            //setting the vector between two points up
            renderer = vector.gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/vector"));
            renderer.material.SetColor("_Color", new Vector4(1, 1, 0, 1));
            vectorArr[i] = vector;

        }
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
        int pointIndex = 0;
        normPerAttribData = normalize(data);
        for (int i = 0; i < data.Length; i++)
        {
            Color classColor = classColors[i];
            int x = data[i].Length;
            for (int j = 0; j < data[i].Length; j++)
            {
                int fxiIndex = ReadFileData.classCount * j + i;
                fxi[fxiIndex] = setFX(aVals, data, i, j);
                fxiParts[fxiIndex] = setFXParts(aVals, data, i, j);

                Material vec = vectorArr[fxiIndex].GetComponent<Renderer>().material;
                bool skipNextMat = false;
                bool outsideRules = false;
                //points
                for (int k = 0; k < NUM_PAIR_COORD; k++)
                {
                    coordPointArr[pointIndex].transform.localPosition = new Vector3((float)normPerAttribData[i][j][k * 2] * X_SCALE - xShift, (float)normPerAttribData[i][j][k * 2 + 1] * Y_SCALE - yShift, (float)(fxi[fxiIndex] * VALUE_TO_CUBE_FRAC_SCALE) - zShift);
                    attribContribPointArr[pointIndex].transform.localPosition = new Vector3((float)normPerAttribData[i][j][k * 2] * X_SCALE - xShift, (float)normPerAttribData[i][j][k * 2 + 1] * Y_SCALE - yShift, (float)(fxiParts[fxiIndex][k] * VALUE_TO_CUBE_FRAC_SCALE) - zShift);
                    Vector3 currentCoord = coordPointArr[pointIndex].transform.localPosition;

                    int otherPointIndex;
                    if (k % 2 == 0)
                    {
                        vec.SetVector("_Pos1", coordPointArr[pointIndex].transform.position);
                        otherPointIndex = 1;
                    }
                    else
                    {
                        vec.SetVector("_Pos2", coordPointArr[pointIndex].transform.position);
                        otherPointIndex = -1;
                    }
                    GameObject[] connectedPoints = { coordPointArr[pointIndex + otherPointIndex], coordPointArr[pointIndex] };
                    GameObject[] connectedAttribPoints = { attribContribPointArr[pointIndex + otherPointIndex], attribContribPointArr[pointIndex] };
                    Material point = coordPointArr[pointIndex].GetComponent<Renderer>().material;
                    Material otherPoint = coordPointArr[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    Material attribPoint = attribContribPointArr[pointIndex].GetComponent<Renderer>().material;
                    Material otherAttribPoint = attribContribPointArr[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    Material coordLines = coordPlaneArr[pointIndex].GetComponent<Renderer>().material;
                    Material otherCoordLine = coordPlaneArr[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    setCoordPlane(coordLines, currentCoord, (float)(c * high_fx) * VALUE_TO_CUBE_FRAC_SCALE - 2.5f);
                    bool ruleCoordOne = (ruleC1[0] >= (float)normPerAttribData[i][j][k * 2] && (float)normPerAttribData[i][j][k * 2] >= ruleC1[1] &&
                            ruleC1[2] >= (float)normPerAttribData[i][j][k * 2 + 1] && (float)normPerAttribData[i][j][k * 2 + 1] >= ruleC1[3]) && k % 2 == 1;
                    bool ruleCoordTwo = (ruleC2[0] >= (float)normPerAttribData[i][j][k * 2] && (float)normPerAttribData[i][j][k * 2] >= ruleC2[1] &&
                            ruleC2[2] >= (float)normPerAttribData[i][j][k * 2 + 1] && (float)normPerAttribData[i][j][k * 2 + 1] >= ruleC2[3]) && k % 2 == 0;
                    if (!skipNextMat)
                    {
                        if (ruleCoordOne || ruleCoordTwo)
                        {
                            attribPoint.SetColor("_Color", new Color(0.7f, 0.7f, 1));
                            coordLines.SetInt("faded", 0);
                            if (fxi[j] <= (c * high_fx))
                            {
                                point.SetColor("_Color", classColor);//class 2
                                vec.SetColor("_Color", classColor);
                            }
                            else
                            {
                                point.SetColor("_Color", classColor); //class 1
                                vec.SetColor("_Color", classColor);
                            }
                            for (int p = 0; p < connectedPoints.Length; p++)
                            {
                                connectedPoints[p].transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                                connectedAttribPoints[p].transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                            }
                        }
                        else
                        {
                            coordLines.SetInt("faded", 1);
                            otherCoordLine.SetInt("faded", 1);
                            point.SetColor("_Color", grey);
                            otherPoint.SetColor("_Color", grey);
                            attribPoint.SetColor("_Color", grey);
                            otherAttribPoint.SetColor("_Color", grey);
                            for (int p = 0; p < connectedPoints.Length; p++)
                            {
                                connectedPoints[p].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                                connectedAttribPoints[p].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            }
                            outsideRules = true;
                            skipNextMat = true;
                        }
                    }
                    pointIndex++;
                }
                if (outsideRules)
                {
                    vectorArr[fxiIndex].GetComponent<Renderer>().enabled = false;
                }
                else
                {
                    vectorArr[fxiIndex].GetComponent<Renderer>().enabled = true;
                }
                //n += 2 * ReadFileData.classCount;
                //draws the connecting line for points
                //vector[0].SetVector("_Pos1", coordPoint[0].transform.position);
                //vector[0].SetVector("_Pos2", coordPoint[1].transform.position);
                //v
                //ector[0].SetVector("_Pos3", coordPoint[2].transform.position);
            }
        }
    }
    //calcultes f(x) based off "A" vector and "X" vector
    //will have to create dynamic "a" index. 
    float setFX(float[] a, float[][][] x, int c, int set)
    {
        float f_x = a[0] * x[c][set][0] + a[1] * x[c][set][1] + a[2] * x[c][set][2] + a[3] * x[c][set][3];
        if(f_x > high_fx)
        {
            high_fx = f_x;
            VALUE_TO_CUBE_FRAC_SCALE = 5f / high_fx;
        }
        return f_x;
    }

    float[] setFXParts(float[] a, float[][][] x, int c, int set)
    {
        float[] f_x_PerSubCoord = { a[0] * x[c][set][0] + a[1] * x[c][set][1], a[2] * x[c][set][2] + a[3] * x[c][set][3]};

        return f_x_PerSubCoord;
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
            float newC = (float)-(c * high_fx) * VALUE_TO_CUBE_FRAC_SCALE; //for fitting purposes into the cubes of the subcoordinates. Otherwise it does not match Fx in heigh
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
        lines.SetVector("_Pos0", new Vector3(-xShift, pos.y, -zShift /*+ 0.01f*/));
        lines.SetVector("_Pos1", new Vector3(pos.x /*- 0.01f*/, -yShift, -zShift /*+ 0.01f*/));
        lines.SetVector("_Pos2", new Vector3(pos.x /*- 0.01f*/, pos.y, -zShift/*+ 0.01f*/));
        lines.SetVector("_Pos3", new Vector3(pos.x, pos.y /*+ 0.01f*/, pos.z));
        lines.SetVector("_C", new Vector3(pos.x, pos.y /*+ 0.01f*/, C));
    }

    float[][][] normalize(float[][][] x)
    {
        float[][][] normArr = new float[x.Length][][];

        float[] maxs = ReadFileData.maxAttribNums;

        for(int i = 0; i < normArr.Length; i++)
        {
            normArr[i] = new float[x[i].Length][];
            for (int j = 0; j < normArr[i].Length; j++)
            {
                normArr[i][j] = new float[x[i][j].Length];
                for (int k = 0; k < normArr[i][j].Length; k++)
                {
                    normArr[i][j][k] = x[i][j][k] / maxs[k];
                }
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
