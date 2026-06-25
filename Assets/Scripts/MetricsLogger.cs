using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Grava métricas da simulação em arquivos CSV para análise externa / criação de gráficos.
///
/// Cada run cria seu próprio diretório em Metrics/&lt;prefix&gt;_&lt;timestamp&gt;/ e grava os
/// CSVs num subdiretório csv/ (os scripts Python depositam plots/ e relatorio.xlsx ao lado):
///   csv/groups.csv         — uma linha por grupo por amostra (coesão, tamanho, afinidade, tempo).
///   csv/summary.csv        — uma linha global por amostra (nº de grupos, solos, trocas).
///   csv/groups_excel.csv   — mesma coisa, mas no formato pt-BR (separador ';', decimal ',').
///   csv/summary_excel.csv  —   "        "
///
/// Os *.csv padrão usam CultureInfo.InvariantCulture (vírgula separa colunas, ponto decimal):
/// lidos direto por pandas / o script tools/plot_metrics.py e por Excel via "De Texto/CSV".
/// Os *_excel.csv usam ';' e ',' → abrem com duplo-clique no Excel em locale pt-BR.
///
/// O World chama BeginSession() ao carregar o mundo, WriteGroupSample/WriteSummarySample
/// a cada eval cycle de grupo, e EndSession() ao encerrar.
/// </summary>
public class MetricsLogger : MonoBehaviour
{
    [Header("Metrics Logging")]
    [SerializeField] private bool LOG_METRICS = true;
    [SerializeField] private string FILE_NAME_PREFIX = "biocrowds_metrics";
    [Tooltip("Também grava cópias *_excel.csv no formato pt-BR (; e ,) para abrir no Excel com duplo-clique.")]
    [SerializeField] private bool WRITE_EXCEL_COPY = true;
    [Tooltip("Grava positions.csv (time,agentId,x,z,groupId) para mapas de trajetória/densidade.")]
    [SerializeField] private bool LOG_POSITIONS = true;

    // writers do formato padrão (, e .)
    private StreamWriter _groupsWriter;
    private StreamWriter _summaryWriter;
    // writers do formato pt-BR (; e ,) — só se WRITE_EXCEL_COPY
    private StreamWriter _groupsWriterXl;
    private StreamWriter _summaryWriterXl;
    private StreamWriter _positionsWriter;
    private string _runDir;   // raiz da run (contém csv/, plots/, relatorio.xlsx)
    private string _csvDir;   // <runDir>/csv — onde ficam todos os .csv
    private bool _open;

    public bool LoggingEnabled => LOG_METRICS;

    /// <summary>Cria o diretório do run e abre os CSVs com cabeçalhos. Chamado pelo World.</summary>
    public void BeginSession()
    {
        if (!LOG_METRICS || _open)
            return;

        // Raiz do projeto (pasta acima de Assets/). No Editor cai dentro do repo;
        // em build, dataPath aponta para a pasta do executável (fallback aceitável).
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        // DateTime.Now é válido em runtime Unity (a restrição é só nos workflow scripts).
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string runName = $"{FILE_NAME_PREFIX}_{stamp}";

        // um diretório por run — não mistura arquivos de runs diferentes.
        // Estrutura: <runDir>/csv (todos os .csv), <runDir>/plots (PNGs), <runDir>/relatorio.xlsx
        string runDir = Path.Combine(projectRoot, "Metrics", runName);
        string csvDir = Path.Combine(runDir, "csv");
        Directory.CreateDirectory(csvDir);
        _runDir = runDir;
        _csvDir = csvDir;

        const string groupsHeader = "time,groupId,groupSize,cohesion,meanAffinity,affinityStdDev,meanTimeInGroup";
        const string summaryHeader = "time,numAgents,numGroups,numSolo,switchesInterval,totalSwitches,groupChangesEnabled,numStuck";

        _groupsWriter = new StreamWriter(Path.Combine(csvDir, "groups.csv"), false);
        _summaryWriter = new StreamWriter(Path.Combine(csvDir, "summary.csv"), false);
        _groupsWriter.WriteLine(groupsHeader);
        _summaryWriter.WriteLine(summaryHeader);

        if (WRITE_EXCEL_COPY)
        {
            _groupsWriterXl = new StreamWriter(Path.Combine(csvDir, "groups_excel.csv"), false);
            _summaryWriterXl = new StreamWriter(Path.Combine(csvDir, "summary_excel.csv"), false);
            _groupsWriterXl.WriteLine(ToExcel(groupsHeader));
            _summaryWriterXl.WriteLine(ToExcel(summaryHeader));
        }

        if (LOG_POSITIONS)
        {
            _positionsWriter = new StreamWriter(Path.Combine(csvDir, "positions.csv"), false);
            _positionsWriter.WriteLine("time,agentId,x,z,groupId");
        }

        _open = true;

        Debug.Log($"[Metrics] run dir:\n  {runDir}");
    }

    public void WriteGroupSample(float time, int groupId, int groupSize, float cohesion, float meanAffinity, float affinityStdDev, float meanTimeInGroup)
    {
        if (!_open) return;
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3:F4},{4:F4},{5:F4},{6:F3}",
            time, groupId, groupSize, cohesion, meanAffinity, affinityStdDev, meanTimeInGroup);
        WriteBoth(_groupsWriter, _groupsWriterXl, line);
    }

    public void WriteSummarySample(float time, int numAgents, int numGroups, int numSolo, int switchesInterval, int totalSwitches, bool groupChangesEnabled, int numStuck)
    {
        if (!_open) return;
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3},{4},{5},{6},{7}",
            time, numAgents, numGroups, numSolo, switchesInterval, totalSwitches, groupChangesEnabled ? 1 : 0, numStuck);
        WriteBoth(_summaryWriter, _summaryWriterXl, line);
    }

    public void WritePositionSample(float time, int agentId, float x, float z, int groupId)
    {
        if (_positionsWriter == null) return;
        _positionsWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2:F3},{3:F3},{4}", time, agentId, x, z, groupId));
    }

    /// <summary>Grava config.csv (key,value) com os parâmetros da run. Chamado uma vez após BeginSession.</summary>
    public void WriteRunConfig(string csvText)
    {
        if (!_open || string.IsNullOrEmpty(_csvDir) || string.IsNullOrEmpty(csvText)) return;
        File.WriteAllText(Path.Combine(_csvDir, "config.csv"), csvText);
        if (WRITE_EXCEL_COPY)
            File.WriteAllText(Path.Combine(_csvDir, "config_excel.csv"), ToExcel(csvText));
    }

    private void WriteBoth(StreamWriter std, StreamWriter xl, string stdLine)
    {
        std.WriteLine(stdLine);
        if (xl != null)
            xl.WriteLine(ToExcel(stdLine));
    }

    // padrão (, e .) -> pt-BR (; e ,). Ordem importa: vírgula->';' antes de ponto->',',
    // pois após a 1ª troca não sobra vírgula, só pontos decimais.
    private static string ToExcel(string stdLine)
    {
        return stdLine.Replace(',', ';').Replace('.', ',');
    }

    public void EndSession()
    {
        if (!_open) return;

        CloseWriter(ref _groupsWriter);
        CloseWriter(ref _summaryWriter);
        CloseWriter(ref _groupsWriterXl);
        CloseWriter(ref _summaryWriterXl);
        CloseWriter(ref _positionsWriter);

        _open = false;
    }

    private static void CloseWriter(ref StreamWriter w)
    {
        if (w == null) return;
        w.Flush();
        w.Close();
        w = null;
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
