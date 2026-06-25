using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Grava métricas da simulação em arquivos CSV para análise externa / criação de gráficos.
///
/// Cada run cria seu próprio diretório em Metrics/&lt;prefix&gt;_&lt;timestamp&gt;/ contendo:
///   groups.csv         — uma linha por grupo por amostra (coesão, tamanho, afinidade, tempo).
///   summary.csv        — uma linha global por amostra (nº de grupos, solos, trocas).
///   groups_excel.csv   — mesma coisa, mas no formato pt-BR (separador ';', decimal ',').
///   summary_excel.csv  —   "        "
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

    // writers do formato padrão (, e .)
    private StreamWriter _groupsWriter;
    private StreamWriter _summaryWriter;
    // writers do formato pt-BR (; e ,) — só se WRITE_EXCEL_COPY
    private StreamWriter _groupsWriterXl;
    private StreamWriter _summaryWriterXl;
    private string _runDir;
    private bool _open;

    public bool LoggingEnabled => LOG_METRICS;

    /// <summary>
    /// Cria o diretório do run e abre os CSVs, escrevendo os cabeçalhos. Chamado pelo World
    /// ao carregar o mundo. Sem efeito se LOG_METRICS estiver desligado ou já houver sessão aberta.
    /// </summary>
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

        // um diretório por run — não mistura arquivos de runs diferentes
        string runDir = Path.Combine(projectRoot, "Metrics", runName);
        Directory.CreateDirectory(runDir);
        _runDir = runDir;

        const string groupsHeader = "time,groupId,groupSize,cohesion,meanAffinity,affinityStdDev,meanTimeInGroup";
        const string summaryHeader = "time,numAgents,numGroups,numSolo,switchesInterval,totalSwitches,groupChangesEnabled,numStuck";

        _groupsWriter = new StreamWriter(Path.Combine(runDir, "groups.csv"), false);
        _summaryWriter = new StreamWriter(Path.Combine(runDir, "summary.csv"), false);
        _groupsWriter.WriteLine(groupsHeader);
        _summaryWriter.WriteLine(summaryHeader);

        if (WRITE_EXCEL_COPY)
        {
            _groupsWriterXl = new StreamWriter(Path.Combine(runDir, "groups_excel.csv"), false);
            _summaryWriterXl = new StreamWriter(Path.Combine(runDir, "summary_excel.csv"), false);
            _groupsWriterXl.WriteLine(ToExcel(groupsHeader));
            _summaryWriterXl.WriteLine(ToExcel(summaryHeader));
        }

        _open = true;

        Debug.Log($"[Metrics] run dir:\n  {runDir}");
    }

    /// <summary>Escreve uma linha de métricas de um grupo (long format).</summary>
    public void WriteGroupSample(float time, int groupId, int groupSize, float cohesion, float meanAffinity, float affinityStdDev, float meanTimeInGroup)
    {
        if (!_open) return;
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3:F4},{4:F4},{5:F4},{6:F3}",
            time, groupId, groupSize, cohesion, meanAffinity, affinityStdDev, meanTimeInGroup);
        WriteBoth(_groupsWriter, _groupsWriterXl, line);
    }

    /// <summary>Escreve a linha-resumo global da amostra.</summary>
    public void WriteSummarySample(float time, int numAgents, int numGroups, int numSolo, int switchesInterval, int totalSwitches, bool groupChangesEnabled, int numStuck)
    {
        if (!_open) return;
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1},{2},{3},{4},{5},{6},{7}",
            time, numAgents, numGroups, numSolo, switchesInterval, totalSwitches, groupChangesEnabled ? 1 : 0, numStuck);
        WriteBoth(_summaryWriter, _summaryWriterXl, line);
    }

    /// <summary>
    /// Grava o config.csv (key,value) com os parâmetros da run. Chamado uma vez pelo World
    /// logo após BeginSession. Também escreve config_excel.csv se WRITE_EXCEL_COPY.
    /// </summary>
    public void WriteRunConfig(string csvText)
    {
        if (!_open || string.IsNullOrEmpty(_runDir) || string.IsNullOrEmpty(csvText)) return;
        File.WriteAllText(Path.Combine(_runDir, "config.csv"), csvText);
        if (WRITE_EXCEL_COPY)
            File.WriteAllText(Path.Combine(_runDir, "config_excel.csv"), ToExcel(csvText));
    }

    /// <summary>Escreve a linha padrão e, se habilitado, a versão pt-BR.</summary>
    private void WriteBoth(StreamWriter std, StreamWriter xl, string stdLine)
    {
        std.WriteLine(stdLine);
        if (xl != null)
            xl.WriteLine(ToExcel(stdLine));
    }

    /// <summary>
    /// Converte uma linha do formato padrão (, e .) para o formato pt-BR (; e ,).
    /// Ordem importa: troca primeiro as vírgulas-separador por ';', depois os pontos-decimal por ','.
    /// (após a 1ª troca não sobra vírgula; só restam pontos decimais.)
    /// </summary>
    private static string ToExcel(string stdLine)
    {
        return stdLine.Replace(',', ';').Replace('.', ',');
    }

    /// <summary>Fecha os arquivos. Chamado pelo World ao encerrar e nos callbacks do Unity.</summary>
    public void EndSession()
    {
        if (!_open) return;

        CloseWriter(ref _groupsWriter);
        CloseWriter(ref _summaryWriter);
        CloseWriter(ref _groupsWriterXl);
        CloseWriter(ref _summaryWriterXl);

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
