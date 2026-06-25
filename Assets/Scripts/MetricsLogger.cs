using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Grava métricas da simulação em arquivos CSV para análise externa / criação de gráficos.
///
/// Dois arquivos por sessão (timestamp no nome):
///   *_groups.csv   — uma linha por grupo por amostra (coesão, tamanho, afinidade).
///   *_summary.csv  — uma linha global por amostra (nº de grupos, solos, trocas).
///
/// O World chama BeginSession() ao carregar o mundo, WriteGroupSample/WriteSummarySample
/// a cada eval cycle de grupo, e EndSession() ao encerrar.
///
/// Floats são formatados com CultureInfo.InvariantCulture (ponto decimal) para que
/// planilhas / pandas leiam corretamente, independente do locale do sistema.
/// </summary>
public class MetricsLogger : MonoBehaviour
{
    [Header("Metrics Logging")]
    [SerializeField] private bool LOG_METRICS = true;
    [SerializeField] private string FILE_NAME_PREFIX = "biocrowds_metrics";

    private StreamWriter _groupsWriter;
    private StreamWriter _summaryWriter;
    private bool _open;

    public bool LoggingEnabled => LOG_METRICS;

    /// <summary>
    /// Abre os arquivos CSV e escreve os cabeçalhos. Chamado pelo World ao carregar o mundo.
    /// Sem efeito se LOG_METRICS estiver desligado ou se já houver sessão aberta.
    /// </summary>
    public void BeginSession()
    {
        if (!LOG_METRICS || _open)
            return;

        // Raiz do projeto (pasta acima de Assets/). No Editor cai dentro do repo;
        // em build, dataPath aponta para a pasta do executável (fallback aceitável).
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string dir = Path.Combine(projectRoot, "Metrics");
        Directory.CreateDirectory(dir);

        // DateTime.Now é válido em runtime Unity (a restrição é só nos workflow scripts).
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string groupsPath = Path.Combine(dir, $"{FILE_NAME_PREFIX}_{stamp}_groups.csv");
        string summaryPath = Path.Combine(dir, $"{FILE_NAME_PREFIX}_{stamp}_summary.csv");

        _groupsWriter = new StreamWriter(groupsPath, false);
        _summaryWriter = new StreamWriter(summaryPath, false);

        _groupsWriter.WriteLine("time,groupId,groupSize,cohesion,meanAffinity,affinityStdDev,meanTimeInGroup");
        _summaryWriter.WriteLine("time,numAgents,numGroups,numSolo,switchesInterval,totalSwitches,groupChangesEnabled");

        _open = true;

        Debug.Log($"[Metrics] logging to:\n  {groupsPath}\n  {summaryPath}");
    }

    /// <summary>Escreve uma linha de métricas de um grupo (long format).</summary>
    public void WriteGroupSample(float time, int groupId, int groupSize, float cohesion, float meanAffinity, float affinityStdDev, float meanTimeInGroup)
    {
        if (!_open) return;
        _groupsWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3:F4},{4:F4},{5:F4},{6:F3}",
            time, groupId, groupSize, cohesion, meanAffinity, affinityStdDev, meanTimeInGroup));
    }

    /// <summary>Escreve a linha-resumo global da amostra.</summary>
    public void WriteSummarySample(float time, int numAgents, int numGroups, int numSolo, int switchesInterval, int totalSwitches, bool groupChangesEnabled)
    {
        if (!_open) return;
        _summaryWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3},{4},{5},{6}",
            time, numAgents, numGroups, numSolo, switchesInterval, totalSwitches, groupChangesEnabled ? 1 : 0));
    }

    /// <summary>Fecha os arquivos. Chamado pelo World ao encerrar e nos callbacks do Unity.</summary>
    public void EndSession()
    {
        if (!_open) return;

        _groupsWriter.Flush();
        _groupsWriter.Close();
        _summaryWriter.Flush();
        _summaryWriter.Close();

        _groupsWriter = null;
        _summaryWriter = null;
        _open = false;
    }

    private void OnDisable()
    {
        EndSession();
    }

    private void OnApplicationQuit()
    {
        EndSession();
    }
}
