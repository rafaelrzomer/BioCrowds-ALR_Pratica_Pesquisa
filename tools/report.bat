@echo off
REM Gera o relatorio .xlsx e os graficos PNG do run MAIS RECENTE em Metrics/.
REM Duplo-clique neste arquivo depois de rodar a simulacao no Unity.
REM Requer Python + libs: pip install pandas matplotlib xlsxwriter

cd /d "%~dp0\.."

echo == Gerando relatorio .xlsx (graficos nativos + formulas) ==
python tools\build_xlsx.py
if errorlevel 1 goto erro

echo.
echo == Gerando graficos PNG ==
python tools\plot_metrics.py
if errorlevel 1 goto erro

echo.
echo == Gerando mapas de trajetoria e densidade ==
python tools\plot_trajectories.py --ask-smooth
if errorlevel 1 goto erro

echo.
echo Concluido. Veja a pasta Metrics\[run-mais-recente]\
pause
exit /b 0

:erro
echo.
echo ERRO ao gerar. Confira se o Python esta instalado e as libs:
echo   pip install pandas matplotlib xlsxwriter
pause
exit /b 1
