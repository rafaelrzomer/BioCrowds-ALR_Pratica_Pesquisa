using Biocrowds.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Poisson-disk sampling (algoritmo de Bridson, 2007) por celula.
// Diferente do DartThrowingMarkerSpawner (rejeicao pura), este mantem uma "lista ativa"
// de amostras e gera candidatos num anel [r, 2r] ao redor delas. Resultado: blue noise
// com espacamento minimo garantido e sem regioes vazias grandes.
public class PoissonDiskMarkerSpawner : MarkerSpawner
{
    // candidatos testados por amostra ativa antes de descarta-la (valor classico de Bridson).
    private const int _candidatesPerSample = 30;

    public override IEnumerator CreateMarkers(List<Cell> cells, List<Auxin> auxins)
    {
        _auxinsContainer = new GameObject("Markers").transform;
        _cellSize = cells[0].transform.localScale.x;

        _maxMarkersPerCell = Mathf.RoundToInt(MarkerDensity / (MarkerRadius * MarkerRadius));

        for (int c = 0; c < cells.Count; c++)
        {
            StartCoroutine(PopulateCell(cells[c], auxins, c));
        }

        yield break;
    }

    private IEnumerator PopulateCell(Cell cell, List<Auxin> auxins, int cellIndex)
    {
        // distancia minima entre marcadores (mesmo criterio de HasMarkersNearby).
        float minDist = MarkerRadius;
        float cellHalfSize = (_cellSize / 2.0f) * (1.0f - (MarkerRadius / 2f));

        // lista ativa: amostras ainda capazes de gerar vizinhas.
        List<Vector3> active = new List<Vector3>();
        int placed = 0;

        // semente inicial da celula (primeira amostra valida encontrada).
        Vector3 seed = FindSeed(cell, cellHalfSize);
        if (PlaceMarker(seed, cell, auxins, cellIndex, placed))
        {
            active.Add(seed);
            placed++;
        }

        while (active.Count > 0 && placed < _maxMarkersPerCell)
        {
            int activeIndex = Random.Range(0, active.Count);
            Vector3 origin = active[activeIndex];
            bool found = false;

            for (int k = 0; k < _candidatesPerSample; k++)
            {
                // candidato num anel [minDist, 2*minDist] ao redor da amostra ativa.
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(minDist, 2f * minDist);
                Vector3 candidate = origin + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                if (!IsInsideCell(candidate, cell, cellHalfSize))
                    continue;
                if (HasObstacleNearby(candidate) || HasMarkersNearby(candidate, cell.Auxins) || !IsOnNavmesh(candidate))
                    continue;

                if (PlaceMarker(candidate, cell, auxins, cellIndex, placed))
                {
                    active.Add(candidate);
                    placed++;
                    found = true;
                    if (placed >= _maxMarkersPerCell)
                        break;
                }
            }

            // amostra esgotada: nenhum candidato valido => sai da lista ativa.
            if (!found)
                active.RemoveAt(activeIndex);
        }

        yield break;
    }

    // Procura uma primeira posicao valida na celula (ate _maxMarkersPerCell * 5 tentativas).
    private Vector3 FindSeed(Cell cell, float cellHalfSize)
    {
        int maxTries = _maxMarkersPerCell * 5;
        for (int t = 0; t < maxTries; t++)
        {
            float x = Random.Range(-cellHalfSize, cellHalfSize);
            float z = Random.Range(-cellHalfSize, cellHalfSize);
            Vector3 candidate = new Vector3(x, 0f, z) + cell.transform.position;

            if (!HasObstacleNearby(candidate) && !HasMarkersNearby(candidate, cell.Auxins) && IsOnNavmesh(candidate))
                return candidate;
        }
        // fallback: centro da celula (sera validado em PlaceMarker).
        return cell.transform.position;
    }

    private bool IsInsideCell(Vector3 pos, Cell cell, float cellHalfSize)
    {
        Vector3 local = pos - cell.transform.position;
        return Mathf.Abs(local.x) <= cellHalfSize && Mathf.Abs(local.z) <= cellHalfSize;
    }

    private bool PlaceMarker(Vector3 pos, Cell cell, List<Auxin> auxins, int cellIndex, int markerIndex)
    {
        if (HasObstacleNearby(pos) || HasMarkersNearby(pos, cell.Auxins) || !IsOnNavmesh(pos))
            return false;

        Auxin newMarker = Instantiate(auxinPrefab, pos, Quaternion.identity, _auxinsContainer);
        newMarker.transform.localScale = Vector3.one * MarkerRadius;
        newMarker.name = "Marker [" + cellIndex + "][" + markerIndex + "]";
        newMarker.Cell = cell;
        newMarker.Position = pos;
        newMarker.ShowMesh(SceneController.ShowAuxins);

        auxins.Add(newMarker);
        cell.Auxins.Add(newMarker);
        return true;
    }
}
