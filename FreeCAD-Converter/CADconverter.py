#!/usr/bin/env python3
import sys
import os
import FreeCAD
import Part
import Mesh

# --- Configurazione directory ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
CAD_DIR = os.path.join(BASE_DIR, "cad_files")
OUT_DIR = os.path.join(BASE_DIR, "converted_obj")

os.makedirs(CAD_DIR, exist_ok=True)
os.makedirs(OUT_DIR, exist_ok=True)

# --- Validazione Input ---
if len(sys.argv) < 3:
    print("Errore: mancano gli argomenti.")
    print("Uso: freecadcmd <script.py> <nome_file_senza_percorso> [tolleranza]")
    sys.exit(1)

file_name = sys.argv[2]
tolerance = 0.1

if len(sys.argv) > 3:
    try:
        tolerance = float(sys.argv[3])
    except ValueError:
        print(f"Errore: la tolleranza '{sys.argv[3]}' non è un numero valido.")
        sys.exit(1)

# --- Composizione percorsi ---
input_file = os.path.join(CAD_DIR, file_name)
if not os.path.exists(input_file):
    print(f"Errore: il file CAD '{file_name}' non esiste in {CAD_DIR}")
    sys.exit(1)

base_name, _ = os.path.splitext(file_name)
output_file = os.path.join(OUT_DIR, base_name + ".obj")

# --- Conversione ---
print(f"Avvio conversione...")
print(f"  Input:  {input_file}")
print(f"  Output: {output_file}")
print(f"  Tolleranza: {tolerance} mm")

try:
    shape = Part.read(input_file)
    if shape.isNull():
        print("Errore: forma nulla, file CAD corrotto o non valido.")
        sys.exit(1)
except Exception as e:
    print(f"Errore durante lettura del file CAD: {e}")
    sys.exit(1)

mesh = Mesh.Mesh()
try:
    mesh.addFacets(shape.tessellate(tolerance))
except Exception as e:
    print(f"Errore durante tessellazione: {e}")
    sys.exit(1)

if mesh.CountFacets == 0:
    print("Errore: mesh vuota (0 triangoli).")
    sys.exit(1)

try:
    mesh.write(output_file, "OBJ")
    print(f"Successo: {output_file}")
    print("-----------------------------------")
    print(f"Poligoni: {mesh.CountFacets}")
    print(f"Vertici:  {mesh.CountPoints}")
    print("-----------------------------------")
except Exception as e:
    print(f"Errore durante esportazione OBJ: {e}")
    sys.exit(1)

sys.exit(0)
