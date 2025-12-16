using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimulationManager))]
public class SimulationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SimulationManager simM = (SimulationManager)target;

        if(GUILayout.Button("StartSim"))
        {
            simM.StartSimulation();
        }
    }
}
