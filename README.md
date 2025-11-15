# CAD Bridge

Il progetto **CAD Bridge** nasce per rispondere alla richiesta della challenge di un'importazione "semplice e immediata" di modelli CAD in ambienti immersivi. L'architettura software prototipale sviluppata permette la conversione e il caricamento dei modelli in **Unity** a runtime e su richiesta, garantendo flessibilità e automazione.

## Stack Tecnologico

La pipeline utilizza strumenti open-source e completamente scriptabili per superare le sfide legate all'importazione CAD:

### 1. Motore di Conversione: FreeCAD
FreeCAD è stato scelto come motore di conversione per i seguenti motivi:
- Open-source e altamente estensibile.
- Basato su kernel CAD robusto (OpenCASCADE), capace di leggere formati industriali come **STEP**, **IGES** e **BREP**.
- Completamente scriptabile da riga di comando tramite `freecadcmd`.

Questo consente di creare una pipeline automatizzata **headless** (senza interfaccia grafica) eseguibile sul PC Host.

### 2. Importazione a Runtime: Runtime OBJ Importer
Per caricare i modelli `.obj` generati nella scena Unity a runtime, è stato previsto l'uso di un importer standard (ad esempio "As-is OBJ loader"), che:
- Esegue il parsing del file `.obj`.
- Crea le mesh in **C#** al momento della richiesta, senza necessità di pre-caricamento.
