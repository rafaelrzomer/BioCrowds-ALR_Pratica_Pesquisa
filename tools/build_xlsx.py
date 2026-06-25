#!/usr/bin/env python3
import argparse
import os
import sys

try:
    import pandas as pd
except ImportError as e:
    sys.exit(f"Dependencia faltando: {e}. Rode: pip install pandas xlsxwriter")


# metricas de grupo: (coluna, titulo da aba/grafico, formato numerico)
# "cohesion" no CSV mede DISPERSAO (dist. media ao centroide): maior = menos coeso.
# Rotulado como "Dispersao" para nao inverter a leitura.
GROUP_METRICS = [
    ("cohesion", "Dispersao", "0.0000"),
    ("meanAffinity", "Afinidade", "0.0000"),
    ("groupSize", "Tamanho", "0"),
    ("meanTimeInGroup", "TempoGrupo", "0.0"),
]


def default_metrics_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(os.path.dirname(here), "Metrics")


def find_latest_run(metrics_dir):
    candidates = []
    for name in os.listdir(metrics_dir):
        sub = os.path.join(metrics_dir, name)
        if os.path.isdir(sub) and os.path.isfile(os.path.join(sub, "summary.csv")):
            candidates.append(sub)
    if not candidates:
        return None
    return os.path.basename(max(candidates, key=os.path.getmtime))


def col_letter(idx):
    """0 -> A, 1 -> B, ... (suficiente para nossas larguras)."""
    s = ""
    idx += 1
    while idx > 0:
        idx, r = divmod(idx - 1, 26)
        s = chr(65 + r) + s
    return s


def write_table(ws, df, fmts, header_fmt, start_row=0, start_col=0):
    """Escreve um DataFrame (com cabecalho) aplicando formatos por coluna. Retorna (n_rows, n_cols)."""
    for c, name in enumerate(df.columns):
        ws.write(start_row, start_col + c, name, header_fmt)
    for r in range(len(df)):
        for c, name in enumerate(df.columns):
            val = df.iat[r, c]
            # NaN (grupo sem dado naquele tempo) -> celula vazia = gap no grafico
            if pd.isna(val):
                ws.write_blank(start_row + 1 + r, start_col + c, None, fmts.get(name))
            else:
                ws.write(start_row + 1 + r, start_col + c, val, fmts.get(name))
    return len(df), len(df.columns)


def add_line_chart(wb, ws, sheet, title, x_axis, y_axis,
                   cat_col, val_cols, n_rows, header_row, anchor, y_min=None):
    chart = wb.add_chart({"type": "line"})
    first = header_row + 1
    last = header_row + n_rows
    cat_ref = [sheet, first, cat_col, last, cat_col]
    for col_idx, serie_name in val_cols:
        chart.add_series({
            "name": serie_name,
            "categories": cat_ref,
            "values": [sheet, first, col_idx, last, col_idx],
            "line": {"width": 1.75},
        })
    chart.set_title({"name": title})
    chart.set_x_axis({"name": x_axis})
    y_axis_opts = {"name": y_axis, "major_gridlines": {"visible": True}}
    if y_min is not None:
        y_axis_opts["min"] = y_min
    chart.set_y_axis(y_axis_opts)
    chart.set_size({"width": 720, "height": 380})
    chart.set_style(2)
    ws.insert_chart(anchor, chart)


def build(run_dir, out_path):
    summary = pd.read_csv(os.path.join(run_dir, "summary.csv"))
    groups_path = os.path.join(run_dir, "groups.csv")
    groups = pd.read_csv(groups_path) if os.path.isfile(groups_path) else pd.DataFrame()

    wb_engine = pd.ExcelWriter(out_path, engine="xlsxwriter")
    wb = wb_engine.book

    # ---- formatos ----
    header_fmt = wb.add_format({"bold": True, "bg_color": "#305496", "font_color": "white",
                                "border": 1, "align": "center", "valign": "vcenter"})
    title_fmt = wb.add_format({"bold": True, "font_size": 14})
    kpi_lbl = wb.add_format({"bold": True, "bg_color": "#D9E1F2", "border": 1})
    kpi_val = wb.add_format({"border": 1, "num_format": "0.00"})
    f_time = wb.add_format({"num_format": "0.000"})
    f_int = wb.add_format({"num_format": "0"})
    f_4 = wb.add_format({"num_format": "0.0000"})
    f_1 = wb.add_format({"num_format": "0.0"})

    # =================== ABA RESUMO ===================
    ws = wb.add_worksheet("Resumo")
    wb_engine.sheets["Resumo"] = ws
    ws.write(0, 0, "BioCrowds — Resumo da Simulacao", title_fmt)
    ws.set_column(0, len(summary.columns) - 1, 14)

    sum_fmts = {
        "time": f_time, "numAgents": f_int, "numGroups": f_int, "numSolo": f_int,
        "switchesInterval": f_int, "totalSwitches": f_int, "groupChangesEnabled": f_int,
    }
    table_row = 2
    n_rows, n_cols = write_table(ws, summary, sum_fmts, header_fmt, start_row=table_row)

    cols = {name: i for i, name in enumerate(summary.columns)}
    # grafico 1: populacao
    series_pop = [(cols[c], c) for c in ("numAgents", "numGroups", "numSolo", "numStuck") if c in cols]
    add_line_chart(wb, ws, "Resumo", "Populacao ao longo do tempo", "Tempo (s)", "Contagem",
                   cols["time"], series_pop, n_rows, table_row, anchor="J3", y_min=0)
    # grafico 2: trocas (eixo a partir de 0 — fica reto em 0 se nao houver trocas)
    if "totalSwitches" in cols:
        add_line_chart(wb, ws, "Resumo", "Trocas de grupo (acumuladas)", "Tempo (s)", "Trocas",
                       cols["time"], [(cols["totalSwitches"], "totalSwitches")],
                       n_rows, table_row, anchor="J24", y_min=0)

    # ---- KPIs com formulas ----
    kpi_row = table_row + n_rows + 3
    data_first = table_row + 2  # 1-based linha Excel do 1o dado
    data_last = table_row + 1 + n_rows
    ws.write(kpi_row, 0, "KPIs (formulas)", title_fmt)

    def colrange(name):
        L = col_letter(cols[name])
        return f"{L}{data_first}:{L}{data_last}"

    kpis = [
        ("Agentes (max)", f"=MAX({colrange('numAgents')})"),
        ("Grupos (media)", f"=AVERAGE({colrange('numGroups')})"),
        ("Solos (media)", f"=AVERAGE({colrange('numSolo')})"),
        ("Trocas (total)", f"=MAX({colrange('totalSwitches')})"),
        ("Tempo final (s)", f"=MAX({colrange('time')})"),
    ]
    for i, (lbl, formula) in enumerate(kpis):
        ws.write(kpi_row + 1 + i, 0, lbl, kpi_lbl)
        ws.write_formula(kpi_row + 1 + i, 1, formula, kpi_val)

    # =================== ABAS POR METRICA DE GRUPO ===================
    if not groups.empty:
        for col, sheet_name, numfmt in GROUP_METRICS:
            if col not in groups.columns:
                continue
            piv = groups.pivot_table(index="time", columns="groupId", values=col).reset_index()
            # renomeia colunas de grupo: 0 -> G0
            piv.columns = ["time"] + [f"G{int(g)}" for g in piv.columns[1:]]

            ws_g = wb.add_worksheet(sheet_name)
            wb_engine.sheets[sheet_name] = ws_g
            ws_g.set_column(0, len(piv.columns) - 1, 12)

            val_fmt = wb.add_format({"num_format": numfmt})
            fmts = {"time": f_time}
            for gname in piv.columns[1:]:
                fmts[gname] = val_fmt
            gr_rows, _ = write_table(ws_g, piv, fmts, header_fmt, start_row=0)

            gcols = {name: i for i, name in enumerate(piv.columns)}
            series = [(gcols[g], g) for g in piv.columns[1:]]
            add_line_chart(wb, ws_g, sheet_name, f"{sheet_name} por grupo", "Tempo (s)", sheet_name,
                           gcols["time"], series, gr_rows, 0,
                           anchor=f"{col_letter(len(piv.columns) + 1)}2")

    # =================== ABA DADOS (raw groups) ===================
    if not groups.empty:
        groups.to_excel(wb_engine, sheet_name="DadosGrupos", index=False)

    # =================== ABA CONFIG (parametros da run) ===================
    config_path = os.path.join(run_dir, "config.csv")
    if os.path.isfile(config_path):
        cfg = pd.read_csv(config_path)
        cfg.to_excel(wb_engine, sheet_name="Config", index=False)

    wb_engine.close()


def main():
    ap = argparse.ArgumentParser(description="Gera relatorio .xlsx dos CSVs de metricas do BioCrowds.")
    ap.add_argument("--dir", default=None, help="Pasta Metrics/ (default: raiz do projeto).")
    ap.add_argument("--run", default=None, help="Nome do subdiretorio do run. Default: mais recente.")
    ap.add_argument("--out", default=None, help="Caminho do .xlsx (default: <run>/relatorio.xlsx).")
    args = ap.parse_args()

    metrics_dir = args.dir or default_metrics_dir()
    if not os.path.isdir(metrics_dir):
        sys.exit(f"Pasta de metricas nao encontrada: {metrics_dir}")

    run = args.run or find_latest_run(metrics_dir)
    if not run:
        sys.exit(f"Nenhum run (subdiretorio com summary.csv) encontrado em {metrics_dir}")

    run_dir = os.path.join(metrics_dir, run)
    if not os.path.isfile(os.path.join(run_dir, "summary.csv")):
        sys.exit(f"summary.csv nao encontrado em {run_dir}")

    out_path = args.out or os.path.join(run_dir, "relatorio.xlsx")

    print(f"Run: {run}")
    build(run_dir, out_path)
    print(f"Gerado: {out_path}")


if __name__ == "__main__":
    main()
