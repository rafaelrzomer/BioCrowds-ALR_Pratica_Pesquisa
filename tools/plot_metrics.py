#!/usr/bin/env python3
"""
Gera os graficos de metricas do BioCrowds (foco em 2 graficos).

O MetricsLogger (Unity) grava CSVs por run na pasta Metrics/ da raiz do projeto:
  summary.csv -> time,numAgents,numGroups,numSolo,switchesInterval,totalSwitches,...
  groups.csv  -> time,groupId,groupSize,cohesion,meanAffinity,affinityStdDev,meanTimeInGroup

Saida (em Metrics/<run>/plots/):
  g1_grupos_solos.png        -> X tempo, Y contagem (numGroups + numSolo)
  g2_dispersao.png           -> dispersao crua por grupo (cohesion) + tamanho sobreposto
  g3_dispersao_normalizada.png -> cohesionNorm = cohesion/sqrt(tam.) (plana = coesao estavel)
  dashboard.png              -> os tres lado a lado

Por que g2 + g3: a dispersao crua (dist. media ao centroide) CRESCE com o tamanho do
grupo (~sqrt(tam.)). Quando um grupo engorda, a linha sobe mesmo sem perder coesao.
A g3 divide por sqrt(tam.) e remove esse efeito: se ficar plana, a coesao por membro
e estavel e a subida da g2 era so crescimento. Ver investigacao de 25/06/2026.

Uso:
  python tools/plot_metrics.py
  python tools/plot_metrics.py --run biocrowds_metrics_20260622_104229
  python tools/plot_metrics.py --dpi 200

Requisitos: pandas, matplotlib  ->  pip install pandas matplotlib
"""

import argparse
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
        ax.legend(loc="best", ncol=min(max(n_groups, 1), 3), title="Grupo")


# ----------------------------- GRAFICO 1 --------------------------------------

def plot_grupos_solos(ax, df):
    """X tempo, Y contagem: numero de grupos e de agentes solos."""
    ax.plot(df["time"], df["numGroups"], drawstyle="steps-post",
            color="#2ca02c", label="Grupos")
    ax.plot(df["time"], df["numSolo"], drawstyle="steps-post",
            color="#ff7f0e", label="Solos")
    style_axis(ax, "Grupos e solos ao longo do tempo", "Tempo (s)", "Numero de grupos", integer_y=True)
    ax.set_ylim(bottom=0)
    ax.legend(loc="best", ncol=2)


# ----------------------------- GRAFICO 2 --------------------------------------

def ensure_cohnorm(df):
    """Garante a coluna cohesionNorm. Runs novas ja gravam; runs antigas calculamos
    aqui (cohesion / sqrt(tamanho)). Sem numpy: pandas faz o ** 0.5 elemento a elemento."""
    if df is None or df.empty:
        return df
    if "cohesionNorm" not in df.columns and {"cohesion", "groupSize"}.issubset(df.columns):
        df = df.copy()
        df["cohesionNorm"] = df["cohesion"] / (df["groupSize"].clip(lower=1) ** 0.5)
    return df


def plot_dispersao(ax, df_groups):
    """X tempo, Y dispersao crua por grupo (dist. media ao centroide; menor = mais coeso),
    com o TAMANHO do grupo sobreposto (tracejado, eixo direito).

    A coluna "cohesion" mede DISPERSAO: valor MAIOR = grupo mais espalhado = MENOS coeso.
    Ela cresce com o tamanho do grupo (~sqrt(tam.)); por isso plotamos o tamanho junto:
    se as duas linhas sobem juntas, a subida da dispersao e artefato de crescimento.
    """
    n = 0
    if df_groups is not None and not df_groups.empty and "cohesion" in df_groups.columns:
        ax2 = ax.twinx()
        for gid, sub in df_groups.groupby("groupId"):
            c = color_for(gid)
            ax.plot(sub["time"], sub["cohesion"], color=c,
                    marker="o", markersize=3, markevery=max(1, len(sub) // 25),
                    label=f"G{int(gid)}")
            if "groupSize" in sub.columns:
                ax2.plot(sub["time"], sub["groupSize"], color=c,
                         linestyle="--", linewidth=1.0, alpha=0.45)
            n += 1
        ax2.set_ylabel("Tamanho do grupo (tracejado)")
        ax2.set_ylim(bottom=0)
        ax2.yaxis.set_major_locator(MaxNLocator(integer=True))
        ax2.spines["top"].set_visible(False)
    style_axis(ax, "Dispersao crua + tamanho (sobem juntos = artefato de tamanho)",
               "Tempo (s)", "Dispersao (dist. media ao centroide)")
    ax.set_ylim(bottom=0)
    if n == 0:
        ax.text(0.5, 0.5, "nenhum grupo formado nesta run",
                transform=ax.transAxes, ha="center", va="center",
                color="#999999", fontsize=11, style="italic")
    else:
        legend_groups(ax, n)


def plot_dispersao_norm(ax, df_groups):
    """X tempo, Y dispersao NORMALIZADA por grupo: cohesion / sqrt(tamanho).

    Remove o efeito do crescimento (a dispersao crua escala ~sqrt(tam.)). Linha plana
    ou em queda => coesao por membro estavel/melhorando; subida => coesao real piorando.
    """
    n = 0
    df = ensure_cohnorm(df_groups)
    if df is not None and not df.empty and "cohesionNorm" in df.columns:
        for gid, sub in df.groupby("groupId"):
            ax.plot(sub["time"], sub["cohesionNorm"], color=color_for(gid),
                    marker="o", markersize=3, markevery=max(1, len(sub) // 25),
                    label=f"G{int(gid)}")
            n += 1
    style_axis(ax, "Dispersao normalizada (/raiz(tam.)) — plana = coesao estavel",
               "Tempo (s)", "Dispersao / sqrt(tamanho)")
    ax.set_ylim(bottom=0)
    if n == 0:
        ax.text(0.5, 0.5, "nenhum grupo formado nesta run",
                transform=ax.transAxes, ha="center", va="center",
                color="#999999", fontsize=11, style="italic")
    else:
        legend_groups(ax, n)


# ----------------------------- IO / ORQUESTRACAO ------------------------------

def default_metrics_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(os.path.dirname(here), "Metrics")


def csv_dir(run_dir):
    """CSVs ficam em <run>/csv/. Fallback p/ <run>/ (runs antigas)."""
    sub = os.path.join(run_dir, "csv")
    return sub if os.path.isdir(sub) else run_dir


def find_latest_run(metrics_dir):
    """Cada run e um subdiretorio de Metrics/ contendo csv/summary.csv. Retorna o mais recente."""
    candidates = []
    for name in os.listdir(metrics_dir):
        sub = os.path.join(metrics_dir, name)
        if os.path.isdir(sub) and os.path.isfile(os.path.join(csv_dir(sub), "summary.csv")):
            candidates.append(sub)
    if not candidates:
        return None
    return os.path.basename(max(candidates, key=os.path.getmtime))


def save(fig, out_dir, fname, dpi):
    path = os.path.join(out_dir, fname)
    fig.savefig(path, dpi=dpi, bbox_inches="tight")
    plt.close(fig)
    print(f"  -> {path}")


def main():
    ap = argparse.ArgumentParser(description="Gera os 2 graficos de metricas do BioCrowds.")
    ap.add_argument("--dir", default=None, help="Pasta com os CSVs (default: ./Metrics na raiz do projeto).")
    ap.add_argument("--run", default=None, help="Nome do subdiretorio do run. Default: mais recente.")
    ap.add_argument("--out", default=None, help="Pasta de saida dos PNGs (default: <Metrics>/<run>/plots).")
    ap.add_argument("--dpi", type=int, default=150, help="Resolucao dos PNGs (default: 150).")
    args = ap.parse_args()

    metrics_dir = args.dir or default_metrics_dir()
    if not os.path.isdir(metrics_dir):
        sys.exit(f"Pasta de metricas nao encontrada: {metrics_dir}")

    run = args.run or find_latest_run(metrics_dir)
    if not run:
        sys.exit(f"Nenhum run (subdiretorio com summary.csv) encontrado em {metrics_dir}")

    run_dir = os.path.join(metrics_dir, run)
    cdir = csv_dir(run_dir)
    summary_path = os.path.join(cdir, "summary.csv")
    groups_path = os.path.join(cdir, "groups.csv")
    if not os.path.isfile(summary_path):
        sys.exit(f"Arquivo nao encontrado: {summary_path}")

    out_dir = args.out or os.path.join(run_dir, "plots")
    os.makedirs(out_dir, exist_ok=True)

    print(f"Run: {run}")
    print(f"Saida: {out_dir}")

    setup_theme()

    df_summary = pd.read_csv(summary_path)
    df_groups = pd.read_csv(groups_path) if os.path.isfile(groups_path) else None
    df_groups = ensure_cohnorm(df_groups)

    # grafico 1
    fig, ax = plt.subplots(figsize=(10, 5))
    plot_grupos_solos(ax, df_summary)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "g1_grupos_solos.png", args.dpi)

    # grafico 2 (dispersao crua + tamanho)
    fig, ax = plt.subplots(figsize=(10, 5))
    plot_dispersao(ax, df_groups)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "g2_dispersao.png", args.dpi)

    # grafico 3 (dispersao normalizada)
    fig, ax = plt.subplots(figsize=(10, 5))
    plot_dispersao_norm(ax, df_groups)
    fig.suptitle(run, fontsize=9, color="#888888", y=0.995)
    save(fig, out_dir, "g3_dispersao_normalizada.png", args.dpi)

    # dashboard (os tres lado a lado)
    fig, axes = plt.subplots(1, 3, figsize=(22, 5.5))
    fig.suptitle(f"BioCrowds — Metricas\n{run}", fontsize=14, fontweight="bold")
    plot_grupos_solos(axes[0], df_summary)
    plot_dispersao(axes[1], df_groups)
    plot_dispersao_norm(axes[2], df_groups)
    fig.tight_layout(rect=(0, 0, 1, 0.93))
    save(fig, out_dir, "dashboard.png", args.dpi)

    print("Concluido.")


if __name__ == "__main__":
    main()
