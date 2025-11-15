using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using Dummiesman;
using System.Collections.Generic;

public class RuntimeCadImporter : MonoBehaviour
{
    [Header("UI")]
    public InputField inputField;

    [Header("Percorsi")]
    public string cadFolder = "/Users/enricomadonna/Desktop/Hackaton/cad_files/";
    public string outputFolder = "/Users/enricomadonna/Desktop/Hackaton/converted_obj/";

    [Header("Parametri Conversione")]
    public float tolerance = 0.1f;      // deviazione di tessellazione
    public float targetSize = 1f;       // scala massima finale del modello

    [Header("Animazione")]
    public Vector3 rotationSpeed = new Vector3(0, 50f, 0);

    private readonly List<GameObject> importedObjects = new List<GameObject>();

    private string freecadCmd = "/Applications/FreeCAD.app/Contents/Resources/bin/freecadcmd";
    private string scriptPath = "/Users/enricomadonna/Desktop/Hackaton/CADconverter.py";

    public void ConvertAndInstantiate()
    {
        string fileName = inputField.text.Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            UnityEngine.Debug.LogError("Inserisci il nome del file CAD (es: modello.step).");
            return;
        }

        string inputPath = Path.Combine(cadFolder, fileName);
        string outputObj = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + ".obj");

        if (!File.Exists(inputPath))
        {
            UnityEngine.Debug.LogError("File CAD non trovato: " + inputPath);
            return;
        }

        string args = $"\"{scriptPath}\" \"{inputPath}\" \"{outputObj}\" {tolerance}";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-l -c \"{freecadCmd} {args}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process p = new Process();
        p.StartInfo = psi;

        try
        {
            p.Start();
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            UnityEngine.Debug.Log("FreeCAD stdout:\n" + stdout);
            if (!string.IsNullOrEmpty(stderr)) UnityEngine.Debug.LogWarning("FreeCAD stderr:\n" + stderr);

            if (p.ExitCode != 0)
            {
                UnityEngine.Debug.LogError("Errore nella conversione CAD → OBJ.");
                return;
            }

            if (File.Exists(outputObj))
            {
                OBJLoader loader = new OBJLoader();
                GameObject objGO = loader.Load(outputObj);

                if (objGO != null)
                {
                    CenterAndScale(objGO, targetSize);
                    objGO.transform.position = Vector3.zero;

                    AutoRotate rotator = objGO.AddComponent<AutoRotate>();
                    rotator.rotationSpeed = rotationSpeed;

                    importedObjects.Add(objGO);

                    UnityEngine.Debug.Log($"OBJ istanziato da {outputObj}, scalato, centrato e autorotante.");
                }
                else
                {
                    UnityEngine.Debug.LogError("Caricamento OBJ fallito: " + outputObj);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("File OBJ non trovato dopo conversione: " + outputObj);
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Errore esecuzione script FreeCAD: " + ex.Message);
        }
    }

    private void CenterAndScale(GameObject go, float targetMaxSize)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 centerOffset = bounds.center - go.transform.position;
        go.transform.position -= centerOffset;

        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDimension > 0f)
            go.transform.localScale = Vector3.one * (targetMaxSize / maxDimension);
    }

    public void DestroyImportedObjects()
    {
        foreach (GameObject obj in importedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        importedObjects.Clear();
        UnityEngine.Debug.Log("Tutti gli oggetti importati sono stati rimossi dalla scena.");
    }
}

public class AutoRotate : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 50f, 0);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}
