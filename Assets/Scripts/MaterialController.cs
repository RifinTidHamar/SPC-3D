using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;

//Handles all Visuals derived from the two vectors, X1 and A. In all honesty this could be split up for organization sake. Ill leave that up to future programmers
public class MaterialController : MonoBehaviour
{
    struct GLCLLine
    {
        public Vector4 GLCLPos;
        public int totalLines;
        public int attribCount;
    }
    private int PAIR_COORDS = 2; //The number of subcoordinates
    private float high_fx = 0;
    private float VALUE_TO_CUBE_FRAC_SCALE; //cubeLengthWidth (always 5 unless scaled in editor) / maximum point height
    private const float X_SCALE = 5;
    private const float Y_SCALE = 5;
    public VisualToggleController visTogElements;
    public GameObject prefabPoint;
    public GameObject prefabVector;
    public GameObject prefabSubCoord;
    public GameObject prefabGLCL;
    public Material rulePlaneC1;
    public Material rulePlaneC2;

    GameObject[] subCoordinateCubes;

    GameObject[] coordPointArr;// = new GameObject[100];
    GameObject[] attribContribPointArr;
    //public Material pointMat;
    GameObject[] classLinePlanes;// = new GameObject[100];
    GameObject[] glclLinePlanes;
    GameObject[] VectorArr;
    //public Text[] restOfFuncTexts;
    Material HeightPlane;
    //public Text fX;


    private ComputeBuffer[] glclBuffer;
    private ComputeBuffer[] vectorBuffer;
    private Vector3 localOffset = new Vector3(2.5f, 2.836596f, 2.5f);
    private Color[] classColors = { new Color(1, 0, 0, 1), new Color(0, 1, 0, 1), new Color(0.5f, 0.5f, 1, 1)};
    private Color class2 = new Color(255, 0, 186, 255);
    private Color class1 = new Color(255, 255, 0, 255);
    private Color grey = new Color(0.2f, 0.2f, 0.2f, 0);
    private float[] fxi;// = new float[50];//f(x) input
    private float[][] fxiParts;// = new float[50][];
    private float[][][] data; //= new float [50,4];
    //data normalized by column
    private float[][][] normPerAttribData;// = new float[50,4];
    private float[] aVals;// = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
    private float[] normAVals;// = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    private float[] ruleC1 = { 1, 0, 1, 0 };
    private float[] ruleC2 = { 1, 0, 1, 0 };
    private float c = 0.5f;
    //private double[] restOfFunction = new double[3]; //ie f12(x) = a1x1 + a2x2 + restOfFunction - f(x)
    private float[,] pointHeight;// = new float[100, 100];

    float xShift = 2.5f;
    float yShift = 2.836596f;
    float zShift = 2.5f;// 0.008552f;

    void Start()
    {
        int nProcessID = Process.GetCurrentProcess().Id;
        UnityEngine.Debug.Log("proc: " + nProcessID);

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { Vector3.zero };
        mesh.triangles = new int[] { 0, 0, 0 };

        //intitialize data
        data = ReadFileData.data;
        fxi = new float[ReadFileData.setCount];
        fxiParts = new float[ReadFileData.setCount][];
        int setCount = ReadFileData.setCount;
        PAIR_COORDS = Mathf.CeilToInt(ReadFileData.attribCount / 2);

        vectorBuffer = new ComputeBuffer[setCount];
        pointHeight = new float[PAIR_COORDS, 4];
        aVals = new float[ReadFileData.attribCount];
        normAVals = new float[ReadFileData.attribCount];

        for (int i = 0; i < ReadFileData.attribCount; i++)
        {
            aVals[i] = 1;
            normAVals[i] = 1;
        }


        //Debug.Log(xVals.GetLength(0));
        subCoordinateCubes = new GameObject[PAIR_COORDS];
        coordPointArr = new GameObject[setCount * PAIR_COORDS];// multiplied by two because it there are two subcoordinates for every 4 values (which makes up one xVal)
        classLinePlanes = new GameObject[setCount * PAIR_COORDS];
        attribContribPointArr = new GameObject[setCount * PAIR_COORDS];
        VectorArr = new GameObject[PAIR_COORDS * setCount];
        GameObject[] tempPoints = new GameObject[setCount * PAIR_COORDS];

        int swapDir = 1;
        Vector3 curPos = new Vector3(0, 0, 0);
        HeightPlane = prefabSubCoord.transform.GetChild(1).gameObject.GetComponent<Renderer>().sharedMaterial;
        for (int i = 0; i < PAIR_COORDS; i++)
        {
            subCoordinateCubes[i] = Instantiate(prefabSubCoord);
            subCoordinateCubes[i].transform.position = curPos;
            //HeightPlane = subCoordinateCubes[i].transform.GetChild(1).gameObject.GetComponent<Renderer>().material;
            if ((i + 1) % 3 == 0 && i > 0)
            {
                swapDir *= -1;
                curPos += new Vector3(0, -10, 0);
                continue;
            }
            curPos += new Vector3(10 * swapDir, 0, 0);
        }
        int pointIndex = 0;
        for (int i = 0; i < setCount; i++)
        {
            vectorBuffer[i] = new ComputeBuffer(PAIR_COORDS, sizeof(float) * 4);
            //tempPoints[i] = Instantiate(prefabPointFirstSubCoord, firstSubCoord.transform);
            //the two points: one in 1st sub coordinate, and one in the second
            for (int j = 0; j < PAIR_COORDS; j++)
            {

                tempPoints[pointIndex] = Instantiate(prefabPoint, subCoordinateCubes[j].transform);
                //setting the two points up
                tempPoints[pointIndex].transform.localPosition = new Vector3(0, 0, 0);
                coordPointArr[pointIndex] = tempPoints[pointIndex].transform.GetChild(1).gameObject;

                attribContribPointArr[pointIndex] = tempPoints[pointIndex].transform.GetChild(2).gameObject;

                //setting the two classLinePlanes up
                tempPoints[pointIndex].transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh = mesh;

                Renderer renderer = tempPoints[pointIndex].transform.GetChild(0).gameObject.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Custom/coordinateLines"));
                renderer.material.renderQueue = 3000;
                renderer.material.SetFloat("_Transparency", 1f);


                classLinePlanes[pointIndex] = tempPoints[pointIndex].transform.GetChild(0).gameObject;


                if (j != j - 1)
                {
                    GameObject vector = Instantiate(prefabVector);


                    vector.gameObject.GetComponent<MeshFilter>().mesh = mesh;

                    //setting the vector between two points up
                    renderer = vector.gameObject.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Custom/vector"));
                    renderer.material.SetColor("_Color", new Vector4(1, 1, 0, 1));

                    VectorArr[pointIndex] = vector;
                }
                pointIndex++;
            }
        }
        visTogElements.attribContrib = attribContribPointArr;
        visTogElements.coordPlanes = classLinePlanes;
        visTogElements.GLCLLines = glclLinePlanes;
        visTogElements.vector = VectorArr;

        updateCubes();
    }
    private void findBufferAndPlaneCount()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { Vector3.zero };
        mesh.triangles = new int[] { 0, 0, 0 };

        int setCount = ReadFileData.setCount;
        int bufferAndPlaneCount = Mathf.CeilToInt((float)(setCount * PAIR_COORDS) / (float)(256));

        glclBuffer = new ComputeBuffer[bufferAndPlaneCount];
        glclLinePlanes = new GameObject[bufferAndPlaneCount];

        for (int i = 0; i < bufferAndPlaneCount; i++)
        {
            if(setCount > 256)
            {
                setCount -= 256;
            }
            int buffercount = setCount > 256 ? 256 : setCount;
            glclBuffer[i] = new ComputeBuffer(buffercount, sizeof(float) * 4 + sizeof(int) * 2);
            GameObject tempGLCL = Instantiate(prefabGLCL);
            tempGLCL.transform.gameObject.GetComponent<MeshFilter>().mesh = mesh;

            Renderer renderer = tempGLCL.transform.gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/GLCL"));
            renderer.material.renderQueue = 3000;
            renderer.material.SetFloat("_Transparency", 1);// 0.002f);

            glclLinePlanes[i] = tempGLCL.transform.gameObject;
        }
    }

    private void updateCubes()
    {
        //rulePlaneC1.SetFloat("_TX", ruleC1[0]);
        //rulePlaneC1.SetFloat("_BX", ruleC1[1]);
        //rulePlaneC1.SetFloat("_TY", ruleC1[2]);
        //rulePlaneC1.SetFloat("_BY", ruleC1[3]);

        //rulePlaneC2.SetFloat("_TX", ruleC2[0]);
        //rulePlaneC2.SetFloat("_BX", ruleC2[1]);
        //rulePlaneC2.SetFloat("_TY", ruleC2[2]);
        //rulePlaneC2.SetFloat("_BY", ruleC2[3]);

        setHeightPlaneCorners();
        normPerAttribData = normalize(data);
        normAVals = normalize(aVals);

        classLines();
    }

    void classLines()
    {
        //for each class
        int addOldIndex = 0;
        int vectorIndex = 0; ;
        //GLCLLine[,] GLCLArr = new GLCLLine[glclBuffer.Length,256];
        for (int i = 0; i < data.Length; i++)
        {
            Color classColor = classColors[i];
            int curLocalIndex = 0;
            //for each set
            for (int j = 0; j < data[i].Length; j++)
            {
                int fxiIndex = j + (i * ReadFileData.classCount);
                fxi[fxiIndex] = setFX(aVals, data, i, j);
                fxiParts[fxiIndex] = setFXParts(aVals, data, i, j);

                Material vec = VectorArr[vectorIndex].GetComponent<Renderer>().material;
                vec.SetColor("_Color", classColor);
                bool skipNextMat = false;
                bool outsideRules = false;
                //for each sub-coordinate
                Vector4[] vecArr = new Vector4[PAIR_COORDS];
                for (int k = 0; k < PAIR_COORDS; k++)
                {
                    int pointIndex = addOldIndex + curLocalIndex;
                    coordPointArr[pointIndex].transform.localPosition = new Vector3((float)normPerAttribData[i][j][k * 2] * X_SCALE - xShift, (float)normPerAttribData[i][j][k * 2 + 1] * Y_SCALE - yShift, (float)(fxi[fxiIndex] * VALUE_TO_CUBE_FRAC_SCALE) - zShift);
                    //attribContribPointArr[pointIndex].transform.localPosition = new Vector3((float)normPerAttribData[i][j][k * 2] * X_SCALE - xShift, (float)normPerAttribData[i][j][k * 2 + 1] * Y_SCALE - yShift, (float)(fxiParts[fxiIndex][k] * VALUE_TO_CUBE_FRAC_SCALE) - zShift);
                    Vector3 currentCoord = coordPointArr[pointIndex].transform.localPosition;
                   
                    //=============================GLCL=========================//
                    Vector3 curCoordXY = new Vector3(currentCoord.x, currentCoord.y, 0);

                    //I don't know what the fuck is going on. I honestly dont care. 
                        //arr[0] = new Vector3(0, 0, 0 - zShift) + curCoordXY;
                        //glclLines.SetColor("_Color", classColor);
                        //glclLines.SetInt("attributeCount", (int)ReadFileData.attribCount);

                        //glclLines.SetVector("_Point", currentCoord);
                        //for(int a = 0; a < ReadFileData.attribCount; a++)
                        //{
                        //    arr[a + 1] = setGLCLPoint(arr[a], data[i][j][a], aVals[a], normAVals[a]);
                        //    //Debug.Log(arr[a + 1]);
                        //}
                        ////ComputeBuffer glclBuffer = new ComputeBuffer(ReadFileData.attribCount + 1, sizeof(float) * 4);
                        //glclBuffer[pointIndex].SetData(arr);
                        //glclLines.SetBuffer("GLCLPos", glclBuffer[pointIndex]);
                    //glclBuffer.Release();
                    //===========================Vector=========================//
                    vecArr[k] = coordPointArr[pointIndex].transform.position;//subCoordinateCubes[i].transform.localToWorldMatrix.MultiplyPoint(arr[arr.Length-1]);
                    //=========================================================//

                    //int otherPointIndex;
                    //Vector3 glclInGlobal = new Vector2(endP.x, endP.y);
                    //if (k % 2 == 0)
                    //{
                    //    //vec.SetVector("_Pos1", coordPointArr[pointIndex].transform.position);
                    //    vec.SetVector("_Pos1", subCoordinateCubes[i].transform.localToWorldMatrix.MultiplyPoint(endP));// + endP - new Vector3(xShift, 0, 0));
                    //    otherPointIndex = 1;
                    //}
                    //else
                    //{
                    //    //vec.SetVector("_Pos2", coordPointArr[pointIndex].transform.position);
                    //    vec.SetVector("_Pos2", subCoordinateCubes[i].transform.localToWorldMatrix.MultiplyPoint(endP));// + endP - new Vector3(xShift, 0, 0));
                    //    otherPointIndex = -1;
                    //}
                    //GameObject[] connectedPoints = { coordPointArr[pointIndex + otherPointIndex], coordPointArr[pointIndex] };
                    //GameObject[] connectedAttribPoints = { attribContribPointArr[pointIndex + otherPointIndex], attribContribPointArr[pointIndex] };
                    Material point = coordPointArr[pointIndex].GetComponent<Renderer>().material;
                    //Material otherPoint = coordPointArr[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    //Material attribPoint = attribContribPointArr[pointIndex].GetComponent<Renderer>().material;
                   // Material otherAttribPoint = attribContribPointArr[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    Material coordLines = classLinePlanes[pointIndex].GetComponent<Renderer>().material;
                   // Material otherCoordLine = classLinePlanes[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    //Material otherGlclLine = glclLinePlanes[pointIndex + otherPointIndex].GetComponent<Renderer>().material;
                    setCoordPlane(coordLines, currentCoord, (float)(c * high_fx) * VALUE_TO_CUBE_FRAC_SCALE - 2.5f);
                    //bool ruleCoordOne = (ruleC1[0] >= (float)normPerAttribData[i][j][k * 2] && (float)normPerAttribData[i][j][k * 2] >= ruleC1[1] &&
                    //        ruleC1[2] >= (float)normPerAttribData[i][j][k * 2 + 1] && (float)normPerAttribData[i][j][k * 2 + 1] >= ruleC1[3]) && k % 2 == 1;
                    //bool ruleCoordTwo = (ruleC2[0] >= (float)normPerAttribData[i][j][k * 2] && (float)normPerAttribData[i][j][k * 2] >= ruleC2[1] &&
                    //        ruleC2[2] >= (float)normPerAttribData[i][j][k * 2 + 1] && (float)normPerAttribData[i][j][k * 2 + 1] >= ruleC2[3]) && k % 2 == 0;
                    //if (!skipNextMat)
                    //{
                    //    if (ruleCoordOne || ruleCoordTwo)
                    //    {

                    //        attribPoint.SetColor("_Color", new Color(1f, 1f, 1));
                    //        coordLines.SetFloat("_Transparency", 0.002f);
                    //        glclLines.SetFloat("_Transparency", 1);
                    //        otherGlclLine.SetFloat("_Transparency", 1);
                    //        point.SetFloat("_Trans", 1);
                    //        otherPoint.SetFloat("_Trans", 1);
                    //        attribPoint.SetFloat("_Trans", 1);
                    //        otherAttribPoint.SetFloat("_Trans", 1);
                    //        if (fxi[j] <= (c * high_fx))
                    //        {
                                  point.SetColor("_Color", classColor);//class 2
                    //            vec.SetColor("_Color", classColor);
                    //        }
                    //        else
                    //        {
                    //            point.SetColor("_Color", classColor); //class 1
                    //            vec.SetColor("_Color", classColor);
                    //        }
                    //        for (int p = 0; p < connectedPoints.Length; p++)
                    //        {
                    //            connectedPoints[p].transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    //            connectedAttribPoints[p].transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        glclLines.SetFloat("_Transparency", 0f);
                    //        otherGlclLine.SetFloat("_Transparency", 0f);
                    //        //coordLines.SetFloat("_Transparency", 0);
                    //        //otherCoordLine.SetFloat("_Transparency", 0);
                    //        point.SetFloat("_Trans", 0);
                    //        otherPoint.SetFloat("_Trans", 0);
                    //        attribPoint.SetFloat("_Trans", 0);
                    //        otherAttribPoint.SetFloat("_Trans", 0);
                    //        for (int p = 0; p < connectedPoints.Length; p++)
                    //        {
                    //            connectedPoints[p].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    //            connectedAttribPoints[p].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    //        }
                    //        outsideRules = true;
                    //        skipNextMat = true;
                    //    }
                    //}
                    curLocalIndex++;
                }
                vectorBuffer[vectorIndex].SetData(vecArr);
                vec.SetBuffer("vecPos", vectorBuffer[vectorIndex]);
                vectorIndex++;

                //if (outsideRules)
                //{
                //    vectorArr[fxiIndex].GetComponent<Renderer>().enabled = false;
                //}
                //else
                //{
                //    vectorArr[fxiIndex].GetComponent<Renderer>().enabled = true;
                //}
            }
            addOldIndex += curLocalIndex;
        }

        //for(int i = 0; i < glclBuffer.Length;i++)
        //{
        //    Vector3 curCoordXY = new Vector3(currentCoord.x, currentCoord.y, 0);
        //    Material glclLines = glclLinePlanes[pointIndex].GetComponent<Renderer>().material;
        //    Vector4[] arr = new Vector4[ReadFileData.attribCount + 1];

        //    arr[0] = new Vector3(0, 0, 0 - zShift) + curCoordXY;
        //    glclLines.SetColor("_Color", classColor);
        //    glclLines.SetInt("attributeCount", (int)ReadFileData.attribCount);

        //    glclLines.SetVector("_Point", currentCoord);
        //    for (int a = 0; a < ReadFileData.attribCount; a++)
        //    {
        //        arr[a + 1] = setGLCLPoint(arr[a], data[i][j][a], aVals[a], normAVals[a]);
        //        //Debug.Log(arr[a + 1]);
        //    }
        //    //ComputeBuffer glclBuffer = new ComputeBuffer(ReadFileData.attribCount + 1, sizeof(float) * 4);
        //    glclBuffer[pointIndex].SetData(arr);
        //    glclLines.SetBuffer("GLCLPos", glclBuffer[pointIndex]);
        //}
    }

    //calcultes f(x) based off "A" vector and "X" vector
    //will have to create dynamic "a" index. 

    Vector3 setGLCLPoint( Vector3 endP, float data, float aval, float normAval)
    {
        float height = (float)(aval * data * VALUE_TO_CUBE_FRAC_SCALE);
        float topRAngle = Mathf.Acos(normAval);
        float bottomLength = data * VALUE_TO_CUBE_FRAC_SCALE * Mathf.Sin(topRAngle);
        endP += new Vector3(bottomLength, 0, height);

        return endP;
    }
    float setFX(float[] a, float[][][] x, int c, int set)
    {
        float f_x = 0;// a[0] * x[c][set][0] + a[1] * x[c][set][1] + a[2] * x[c][set][2] + a[3] * x[c][set][3];
        for(int i = 0; i < ReadFileData.attribCount; i++)
        {
            float A = a[i];
            f_x += A * x[c][set][i];
        }
        if(f_x > high_fx)
        {
            high_fx = f_x;
            VALUE_TO_CUBE_FRAC_SCALE = 5f / high_fx;
        }
        return f_x;
    }

    float[] setFXParts(float[] a, float[][][] x, int c, int set)
    {
        float[] f_x_PerSubCoord = new float[PAIR_COORDS];// { a[0] * x[c][set][0] + a[1] * x[c][set][1], a[2] * x[c][set][2] + a[3] * x[c][set][3]};
        int i = 0;
        int n = 0;
        while(i < PAIR_COORDS)
        {
            f_x_PerSubCoord[n] = a[i] * x[c][set][i] + a[i + 1] * x[c][set][i + 1];
            i += 2;
            n += 1;
        }

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
        float newC = (float)-(c * high_fx) * VALUE_TO_CUBE_FRAC_SCALE; //for fitting purposes into the cubes of the subcoordinates. Otherwise it does not match Fx in heigh
        //pointHeight[i, 0] = newC;// setPointHeight(0, 0, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(0,0)
        //pointHeight[i, 1] = newC;// setPointHeight(1, 0, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(1,0)
        //pointHeight[i, 2] = newC;// setPointHeight(1, 1, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(1,1)
        //pointHeight[i, 3] = newC;// setPointHeight(0, 1, aVals[2 * i], aVals[2 * i + 1], restOfFunction[i]); //(0,1);
        HeightPlane.SetFloat("_Pos0", newC);
        HeightPlane.SetFloat("_Pos1", newC);
        HeightPlane.SetFloat("_Pos2", newC);
        HeightPlane.SetFloat("_Pos3", newC);
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

    float[] normalize(float[] x)
    {
        float[] normArr = new float[x.Length];

        float max = -1;
        foreach(int i in x)
        {
            if(Mathf.Abs(i) > max)
            {
                max = i;
            }
        }

        for (int i = 0; i < normArr.Length; i++)
        {
            normArr[i] =Mathf.Abs(max) > 1? x[i] / max : x[i];
        }
        return normArr;
    }

    //These set of functions read in the A vector from the UI, one element at a time
    public void readA(string aInp, int placement)
    {
        UnityEngine.Debug.Log(placement);
        UnityEngine.Debug.Log(aInp);
        if (float.TryParse(aInp, out _))
            aVals[placement] = float.Parse(aInp);
        updateCubes();
    }

    //reads in the C input
    public void readC(string cInp)
    {
        if (float.TryParse(cInp, out _))
            c = float.Parse(cInp);
        updateCubes();
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
    private void OnDestroy()
    {
        for(int i = 0; i < ReadFileData.setCount; i++)
        {
            //glclBuffer[i].Release();
            //vectorBuffer[i].Release();
        }
    }
}
