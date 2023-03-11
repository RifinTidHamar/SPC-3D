using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Takes in toggles to hide or show given visuals
public class VisualToggleController : MonoBehaviour
{
    public GameObject[] heightPlanes;
    public GameObject[] vector;
    public GameObject[] coordPlanes;
    public GameObject vectorPlaneToggleHelp;
    public GameObject[] pointsBelowPlane;

    private bool heightToggle = true;
    private bool vectorToggle = true;
    private bool coordToggle = true;
    private bool belowPointToggle = true;

    //points below height planes
    public void setBelowPoint()
    {
        belowPointToggle = !belowPointToggle;
        vectorPlaneToggleHelp.SetActive(belowPointToggle);
        for(int i = 0; i < pointsBelowPlane.Length; i++)
        {
            pointsBelowPlane[i].SetActive(belowPointToggle);
        }
    }
    //yellow height planes
    public void setHeightPlanes()
    {
        heightToggle = !heightToggle;
        for(int i = 0; i < heightPlanes.Length; i++)
        {
            heightPlanes[i].gameObject.SetActive(heightToggle);
        }
    }
    //connecting line between subcoordinate points
    public void setVector()
    {
        vectorToggle = !vectorToggle;
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i].SetActive(vectorToggle);
        }
    }
    //rbg coordinates for point
    public void setCoord()
    {
        coordToggle = !coordToggle;
        for(int i = 0; i < coordPlanes.Length; i++)
        {
            coordPlanes[i].gameObject.SetActive(coordToggle);
        }
    }
}
