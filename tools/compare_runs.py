#!/usr/bin/env python3
"""
Compara varias runs do BioCrowds sobrepondo UMA metrica do summary.csv vs tempo.

Util para os resultados do artigo: ex. comparar ALLOW_GROUP_CHANGES on x off,
seeds diferentes, ou densidades diferentes — uma linha por run no mesmo grafico.

Cada run e um subdiretorio de Metrics/ contendo summary.csv (layout do MetricsLogger).

Uso:
  python tools/compare_runs.py                              # todas as runs, metrica numGroups
  python tools/compare_runs.py --metric numStuck            # escolhe a metrica
  python tools/compare_runs.py --last 3 --metric totalSwitches
  python tools/compare_runs.py --runs run_a run_b --metric numGroups
  python tools/compare_runs.py --out comparacao.png

Metricas disponiveis (colunas do summary.csv):
  numAgents, numGroups, numSolo, switchesInterval, totalSwitches, numStuck, groupChangesEnabled

Requisitos: pandas, matplotlib  ->  pip install pandas matplotlib
"""

import argparse
import os
import sys

try:
    import pandas as pd
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from matplotlib.ticker import MaxNLocator
except ImportError as e:
    sys.exit(f"Dependencia faltando: {e}. Rode: pip install pandas matplotlib")


METRIC_LABELS = {
    "numAgents": "Nº de agentes",
    "numGroups": "Nº de grupos",
    "numSolo": "Nº de solos",
    "switchesInterval": "Trocas por intervalo",
    "totalSwitches": "Trocas acumuladas",
    "numStuck": "Agentes travados (jam)",
    "groupChangesEnabled": "ALLOW_GROUP_CHANGES (0/1)",
}


def default_metrics_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(os.path.dirname(here), "Metrics")


def list_runs(metrics_dir):
    """Subdiretorios de Metrics/ com summary.csv, ordenados por mtime (mais novo por ultimo)."""
    runs = []
    for name in sorted(os.listdir(metrics_dir)):
        sub = os.path.join(metrics_dir, name)
        if os.path.isdir(sub) and os.path.isfile(os.path.join(sub, "summary.csv")):
            runs.append(name)
    runs.sort(key=lambda n: os.path.getmtime(os.path.join(metrics_dir, n)))
    return runs


def short_label(run_name):
    """Encurta 'biocrowds_metrics_20260625_002752' -> '20260625_002752'."""
    parts = run_name.split("_")
    return "_".join(parts[-2:]) if len(parts) >= 2 else run_name


def main():
    ap = argparse.ArgumentParser(description="Compara metricas de varias runs do BioCrowds.")
    ap.add_argument("--dir", default=None, help="Pasta Metrics/ (default: raiz do projeto).")
    ap.add_argument("--runs", nargs="+", default=None, help="Nomes dos subdiretorios de run a comparar.")
    ap.add_argument("--last", type=int, default=None, help="Compara as N runs mais recentes.")
    ap.add_argument("--metric", default="numGroups", help="Coluna do summary.csv a comparar.")
    ap.add_argument("--out", default=None, help="Caminho do PNG (default: Metrics/comparisons/compare_<metric>.png).")
    ap.add_argument("--dpi", type=int, default=150)
    args = ap.parse_args()

    metrics_dir = args.dir or default_metrics_dir()
    if not os.path.isdir(metrics_dir):
        sys.exit(f"Pasta de metricas nao encontrada: {metrics_dir}")

    all_runs = list_runs(metrics_dir)
    if not all_runs:
        sys.exit(f"Nenhuma run (subdir com summary.csv) em {metrics_dir}")

    if args.runs:
        runs = [r for r in args.runs if r in all_runs]
        missing = [r for r in args.runs if r not in all_runs]
        for m in missing:
            print(f"[aviso] run nao encontrada, ignorando: {m}")
    elif args.last:
        runs = all_runs[-args.last:]
    else:
        runs = all_runs

    if not runs:
        sys.exit("Nenhuma run valida selecionada.")

    metric = args.metric
    label = METRIC_LABELS.get(metric, metric)

    plt.rcParams.update({"axes.grid": True, "grid.alpha": 0.3, "font.family": "DejaVu Sans"})
    fig, ax = plt.subplots(figsize=(11, 6))

    plotted = 0
    for run in runs:
        df = pd.read_csv(os.path.join(metrics_dir, run, "summary.csv"))
        if metric not in df.columns:
            print(f"[aviso] '{metric}' ausente em {run} (run antiga?); ignorando.")
            continue
        ax.plot(df["time"], df[metric], drawstyle="steps-post", linewidth=1.8, label=short_label(run))
        plotted += 1

    if plotted == 0:
        sys.exit(f"Nenhuma run tem a coluna '{metric}'. Metricas validas: {', '.join(METRIC_LABELS)}")

    ax.set_xlabel("Tempo (s)")
    ax.set_ylabel(label)
    ax.set_title(f"Comparacao entre runs — {label}")
    ax.yaxis.set_major_locator(MaxNLocator(integer=True))
    ax.set_ylim(bottom=0)
    for side in ("top", "right"):
        ax.spines[side].set_visible(False)
    ax.legend(title="Run", fontsize=8, ncol=1, loc="best")

    out_path = args.out
    if not out_path:
        out_dir = os.path.join(metrics_dir, "comparisons")
        os.makedirs(out_dir, exist_ok=True)
        out_path = os.path.join(out_dir, f"compare_{metric}.png")

    fig.tight_layout()
    fig.savefig(out_path, dpi=args.dpi, bbox_inches="tight")
    plt.close(fig)
    print(f"Comparou {plotted} run(s) em '{metric}'.")
    print(f"Gerado: {out_path}")


if __name__ == "__main__":
    main()
