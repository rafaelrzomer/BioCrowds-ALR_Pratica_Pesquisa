#!/usr/bin/env python3
"""
Gera graficos a partir dos CSVs de metricas do BioCrowds.

O MetricsLogger (Unity) grava dois CSVs por run na pasta Metrics/ da raiz do projeto:
  *_summary.csv  -> time,numAgents,numGroups,numSolo,switchesInterval,totalSwitches,groupChangesEnabled
  *_groups.csv   -> time,groupId,groupSize,cohesion,meanAffinity,affinityStdDev,meanTimeInGroup

Este script localiza o par mais recente (ou um run especifico), le com pandas e
salva PNGs em Metrics/plots/<run>/ — graficos individuais + um dashboard combinado.

Uso:
  python tools/plot_metrics.py                      # run mais recente em ./Metrics
  python tools/plot_metrics.py --dir caminho/Metrics
  python tools/plot_metrics.py --run biocrowds_metrics_20260622_104229
  python tools/plot_metrics.py --out figuras/
  python tools/plot_metrics.py --dpi 200            # resolucao dos PNGs

Requisitos: pandas, matplotlib  ->  pip install pandas matplotlib
"""

import argparse
import glob
import os
import sys

try:
    import pandas as pd
    import matplotlib
    matplotlib.use("Agg")  # backend sem display (salva arquivos)
    import matplotlib.pyplot as plt
    from matplotlib.ticker import MaxNLocator
except ImportError as e:
    sys.exit(f"Dependencia faltando: {e}. Rode: pip install pandas matplotlib")


# Paleta qualitativa (Tab10) reaproveitada por groupId de forma estavel.
PALETTE = [
    "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
    "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf",
]


def color_for(gid):
    """Cor estavel por groupId (mesmo grupo => mesma cor em todos os graficos)."""
    return PALETTE[int(gid) % len(PALETTE)]


def setup_theme():
    """Tema visual consistente para todos os graficos."""
    plt.rcParams.update({
        "figure.facecolor": "white",
        "axes.facecolor": "#fbfbfd",
        "axes.edgecolor": "#cccccc",
        "axes.grid": True,
        "axes.axisbelow": True,
        "grid.color": "#e2e2e8",
        "grid.linewidth": 0.8,
        "axes.titlesize": 13,
        "axes.titleweight": "bold",
        "axes.labelsize": 11,
        "xtick.labelsize": 9,
        "ytick.labelsize": 9,
        "legend.fontsize": 8,
        "legend.framealpha": 0.9,
        "lines.linewidth": 2.0,
        "font.family": "DejaVu Sans",
    })


def style_axis(ax, title, xlabel, ylabel, integer_y=False):
    ax.set_title(title)
    ax.set_xlabel(xlabel)
    ax.set_ylabel(ylabel)
    for side in ("top", "right"):
        ax.spines[side].set_visible(False)
    if integer_y:
        ax.yaxis.set_major_locator(MaxNLocator(integer=True))


def legend_groups(ax, n_groups):
    """Legenda dentro do eixo; se houver muitos grupos, joga para fora a direita."""
    if n_groups > 6:
        ax.legend(loc="center left", bbox_to_anchor=(1.01, 0.5), ncol=1, title="Grupo")
    else:
        ax.legend(loc="best", ncol=min(n_groups, 3), title="Grupo")


# ----------------------------- SUMMARY ----------------------------------------

def _plot_population(ax, df):
    ax.plot(df["time"], df["numAgents"], drawstyle="steps-post",
            color="#1f77b4", label="Agentes")
    ax.plot(df["time"], df["numGroups"], drawstyle="steps-post",
            color="#2ca02c", label="Grupos")
    ax.plot(df["time"], df["numSolo"], drawstyle="steps-post",
            color="#ff7f0e", label="Solos")
    style_axis(ax, "Populacao ao longo do tempo", "Tempo (s)", "Contagem", integer_y=True)
    ax.legend(loc="best", ncol=3)


def _plot_switches(ax, df):
    ax.plot(df["time"], df["totalSwitches"], drawstyle="steps-post",
            color="#d62728", label="Acumuladas")
    ax.fill_between(df["time"], df["totalSwitches"], step="post",
                    color="#d62728", alpha=0.12)
    if "switchesInterval" in df.columns:
        ax.bar(df["time"], df["switchesInterval"], width=0.2,
               color="#d62728", alpha=0.45, label="Por intervalo")
    style_axis(ax, "Trocas de grupo", "Tempo (s)", "Trocas", integer_y=True)
    ax.legend(loc="upper left")


def shade_group_changes(ax, df):
    """Sombreia faixas onde a dinamica de grupos esta ON (groupChangesEnabled=1)."""
    if "groupChangesEnabled" not in df.columns:
        return
    if (df["groupChangesEnabled"] == 1).all():
        return  # tudo ON: nao precisa sombrear
    on = df["groupChangesEnabled"] == 1
    ax.fill_between(df["time"], 0, 1, where=on, transform=ax.get_xaxis_transform(),
                    color="#2ca02c", alpha=0.06, step="post")


def plot_summary(df, out_dir, run, dpi):
    fig, ax = plt.subplots(figsize=(10, 5))
    _plot_population(ax, df)
    shade_group_changes(ax, df)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "summary_populacao.png", dpi)

    fig, ax = plt.subplots(figsize=(10, 5))
    _plot_switches(ax, df)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "summary_trocas.png", dpi)


# ----------------------------- GROUPS -----------------------------------------

def _plot_group_lines(ax, df, col):
    groups = list(df.groupby("groupId"))
    for gid, sub in groups:
        ax.plot(sub["time"], sub[col], color=color_for(gid),
                marker="o", markersize=3, markevery=max(1, len(sub) // 25),
                label=f"G{int(gid)}")
    return len(groups)


def _plot_affinity(ax, df):
    """Afinidade media por grupo + banda sombreada de +-1 desvio padrao."""
    groups = list(df.groupby("groupId"))
    for gid, sub in groups:
        c = color_for(gid)
        ax.plot(sub["time"], sub["meanAffinity"], color=c, label=f"G{int(gid)}")
        if "affinityStdDev" in sub.columns:
            lo = sub["meanAffinity"] - sub["affinityStdDev"]
            hi = sub["meanAffinity"] + sub["affinityStdDev"]
            ax.fill_between(sub["time"], lo, hi, color=c, alpha=0.15)
    ax.set_ylim(0, 1)
    return len(groups)


def plot_groups(df, out_dir, run, dpi):
    if df.empty:
        print("[aviso] groups.csv vazio (nenhum grupo formado na run); pulando graficos por grupo.")
        return

    especs = [
        ("cohesion", "Coesao (dist. media ao centroide)", "groups_coesao.png", False),
        ("groupSize", "Tamanho do grupo", "groups_tamanho.png", True),
        ("meanTimeInGroup", "Tempo medio no grupo (s)", "groups_tempo.png", False),
    ]
    for col, titulo, fname, int_y in especs:
        if col not in df.columns:
            continue
        fig, ax = plt.subplots(figsize=(10, 5))
        n = _plot_group_lines(ax, df, col)
        style_axis(ax, f"{titulo} por grupo", "Tempo (s)", titulo, integer_y=int_y)
        legend_groups(ax, n)
        fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
        save(fig, out_dir, fname, dpi)

    # afinidade tem tratamento especial (banda de desvio)
    fig, ax = plt.subplots(figsize=(10, 5))
    n = _plot_affinity(ax, df)
    style_axis(ax, "Afinidade media +- desvio por grupo", "Tempo (s)", "Afinidade [0..1]")
    legend_groups(ax, n)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "groups_afinidade.png", dpi)


# ----------------------------- DASHBOARD --------------------------------------

def plot_dashboard(df_summary, df_groups, out_dir, run, dpi):
    """Painel unico 2x2 com a visao geral da run."""
    fig, axes = plt.subplots(2, 2, figsize=(15, 9))
    fig.suptitle(f"BioCrowds — Dashboard de Metricas\n{run}", fontsize=15, fontweight="bold")

    _plot_population(axes[0, 0], df_summary)
    shade_group_changes(axes[0, 0], df_summary)

    _plot_switches(axes[0, 1], df_summary)

    if df_groups is not None and not df_groups.empty and "cohesion" in df_groups.columns:
        n = _plot_group_lines(axes[1, 0], df_groups, "cohesion")
        style_axis(axes[1, 0], "Coesao por grupo", "Tempo (s)", "Dist. media ao centroide")
        legend_groups(axes[1, 0], n)
    else:
        axes[1, 0].set_axis_off()

    if df_groups is not None and not df_groups.empty:
        n = _plot_affinity(axes[1, 1], df_groups)
        style_axis(axes[1, 1], "Afinidade +- desvio por grupo", "Tempo (s)", "Afinidade [0..1]")
        legend_groups(axes[1, 1], n)
    else:
        axes[1, 1].set_axis_off()

    fig.tight_layout(rect=(0, 0, 1, 0.96))
    save(fig, out_dir, "dashboard.png", dpi)


# ----------------------------- IO ---------------------------------------------

def default_metrics_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(os.path.dirname(here), "Metrics")


def find_latest_run(metrics_dir):
    summaries = glob.glob(os.path.join(metrics_dir, "*_summary.csv"))
    if not summaries:
        return None
    latest = max(summaries, key=os.path.getmtime)
    return os.path.basename(latest)[: -len("_summary.csv")]


def save(fig, out_dir, fname, dpi):
    path = os.path.join(out_dir, fname)
    fig.savefig(path, dpi=dpi, bbox_inches="tight")
    plt.close(fig)
    print(f"  -> {path}")


def main():
    ap = argparse.ArgumentParser(description="Gera graficos dos CSVs de metricas do BioCrowds.")
    ap.add_argument("--dir", default=None, help="Pasta com os CSVs (default: ./Metrics na raiz do projeto).")
    ap.add_argument("--run", default=None, help="Prefixo do run. Default: mais recente.")
    ap.add_argument("--out", default=None, help="Pasta de saida dos PNGs (default: <Metrics>/plots/<run>).")
    ap.add_argument("--dpi", type=int, default=150, help="Resolucao dos PNGs (default: 150).")
    args = ap.parse_args()

    metrics_dir = args.dir or default_metrics_dir()
    if not os.path.isdir(metrics_dir):
        sys.exit(f"Pasta de metricas nao encontrada: {metrics_dir}")

    run = args.run or find_latest_run(metrics_dir)
    if not run:
        sys.exit(f"Nenhum *_summary.csv encontrado em {metrics_dir}")

    summary_path = os.path.join(metrics_dir, f"{run}_summary.csv")
    groups_path = os.path.join(metrics_dir, f"{run}_groups.csv")
    if not os.path.isfile(summary_path):
        sys.exit(f"Arquivo nao encontrado: {summary_path}")

    out_dir = args.out or os.path.join(metrics_dir, "plots", run)
    os.makedirs(out_dir, exist_ok=True)

    print(f"Run: {run}")
    print(f"Saida: {out_dir}")

    setup_theme()

    df_summary = pd.read_csv(summary_path)
    df_groups = pd.read_csv(groups_path) if os.path.isfile(groups_path) else None

    plot_summary(df_summary, out_dir, run, args.dpi)
    if df_groups is not None:
        plot_groups(df_groups, out_dir, run, args.dpi)
    else:
        print(f"[aviso] {groups_path} ausente; so graficos de summary gerados.")
    plot_dashboard(df_summary, df_groups, out_dir, run, args.dpi)

    print("Concluido.")


if __name__ == "__main__":
    main()
