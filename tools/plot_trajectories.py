#!/usr/bin/env python3
"""
Gera mapa de TRAJETÓRIAS e mapa de DENSIDADE a partir do positions.csv de uma run.

O MetricsLogger grava (se LOG_POSITIONS) um positions.csv por run:
  time,agentId,x,z,groupId   — uma linha por agente por amostra (eval cycle).

Saídas em Metrics/<run>/plots/:
  trajectories.png  — caminho (x,z) de cada agente, colorido pelo grupo final.
  density.png       — mapa de calor (histograma 2D) de onde os agentes mais passaram.

Atende o Caderno (pontos de densidade / caminhos preferenciais) e o WebCrowds
(Density Map / Trajectories Map).

Uso:
  python tools/plot_trajectories.py
  python tools/plot_trajectories.py --run biocrowds_metrics_20260625_120000
  python tools/plot_trajectories.py --bins 80 --dpi 200

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
except ImportError as e:
    sys.exit(f"Dependencia faltando: {e}. Rode: pip install pandas matplotlib")


PALETTE = [
    "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
    "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf",
]


def color_for(gid):
    if gid < 0:
        return "#bbbbbb"  # solo
    return PALETTE[int(gid) % len(PALETTE)]


def default_metrics_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(os.path.dirname(here), "Metrics")


def find_latest_run(metrics_dir):
    cands = []
    for name in os.listdir(metrics_dir):
        sub = os.path.join(metrics_dir, name)
        if os.path.isdir(sub) and os.path.isfile(os.path.join(sub, "positions.csv")):
            cands.append(sub)
    if not cands:
        return None
    return os.path.basename(max(cands, key=os.path.getmtime))


def plot_trajectories(df, out_dir, run, dpi):
    fig, ax = plt.subplots(figsize=(9, 9))
    # grupo final de cada agente -> cor estável da trajetória
    last_group = df.sort_values("time").groupby("agentId")["groupId"].last()
    for aid, sub in df.groupby("agentId"):
        sub = sub.sort_values("time")
        ax.plot(sub["x"], sub["z"], color=color_for(last_group.get(aid, -1)),
                linewidth=1.0, alpha=0.6)
        # ponto inicial (o) e final (x)
        ax.plot(sub["x"].iloc[0], sub["z"].iloc[0], "o", color="#2ca02c", markersize=3, alpha=0.5)
        ax.plot(sub["x"].iloc[-1], sub["z"].iloc[-1], "x", color="#d62728", markersize=4, alpha=0.6)
    ax.set_xlabel("x")
    ax.set_ylabel("z")
    ax.set_title(f"Trajetorias dos agentes (cor = grupo final)\n{run}")
    ax.set_aspect("equal", adjustable="datalim")
    ax.grid(True, alpha=0.2)
    _save(fig, out_dir, "trajectories.png", dpi)


def plot_density(df, out_dir, run, dpi, bins):
    fig, ax = plt.subplots(figsize=(9, 8))
    h = ax.hist2d(df["x"], df["z"], bins=bins, cmap="inferno")
    fig.colorbar(h[3], ax=ax, label="amostras na celula (densidade)")
    ax.set_xlabel("x")
    ax.set_ylabel("z")
    ax.set_title(f"Mapa de densidade (onde os agentes mais passaram)\n{run}")
    ax.set_aspect("equal", adjustable="box")
    _save(fig, out_dir, "density.png", dpi)


def _save(fig, out_dir, fname, dpi):
    path = os.path.join(out_dir, fname)
    fig.tight_layout()
    fig.savefig(path, dpi=dpi, bbox_inches="tight")
    plt.close(fig)
    print(f"  -> {path}")


def main():
    ap = argparse.ArgumentParser(description="Gera mapas de trajetoria e densidade do BioCrowds.")
    ap.add_argument("--dir", default=None, help="Pasta Metrics/ (default: raiz do projeto).")
    ap.add_argument("--run", default=None, help="Subdiretorio do run. Default: mais recente com positions.csv.")
    ap.add_argument("--out", default=None, help="Pasta de saida (default: <run>/plots).")
    ap.add_argument("--bins", type=int, default=60, help="Resolucao do mapa de densidade (default: 60).")
    ap.add_argument("--dpi", type=int, default=150)
    args = ap.parse_args()

    metrics_dir = args.dir or default_metrics_dir()
    if not os.path.isdir(metrics_dir):
        sys.exit(f"Pasta de metricas nao encontrada: {metrics_dir}")

    run = args.run or find_latest_run(metrics_dir)
    if not run:
        sys.exit(f"Nenhuma run com positions.csv em {metrics_dir} (LOG_POSITIONS ligado?)")

    pos_path = os.path.join(metrics_dir, run, "positions.csv")
    if not os.path.isfile(pos_path):
        sys.exit(f"positions.csv nao encontrado: {pos_path}")

    out_dir = args.out or os.path.join(metrics_dir, run, "plots")
    os.makedirs(out_dir, exist_ok=True)

    df = pd.read_csv(pos_path)
    if df.empty:
        sys.exit("positions.csv vazio.")

    print(f"Run: {run}  ({len(df)} amostras, {df['agentId'].nunique()} agentes)")
    plot_trajectories(df, out_dir, run, args.dpi)
    plot_density(df, out_dir, run, args.dpi, args.bins)
    print("Concluido.")


if __name__ == "__main__":
    main()
