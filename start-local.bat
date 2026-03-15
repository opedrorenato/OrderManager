@echo off
title OrderManager Local

echo [1/3] Iniciando OrderAccumulator - ConsoleApp
start "Accumulator" cmd /k "cd Backend\OrderManager\OrderAccumulator && echo Accumulator: ConsoleApp && dotnet run"

echo [2/3] Iniciando OrderGenerator (API + Swagger) - https://localhost:5001
start "Generator" cmd /k "cd Backend\OrderManager\OrderGenerator && echo Generator: https://localhost:5001 && dotnet run"

echo [3/3] Iniciando Frontend (Angular) - http://localhost:4200
start "Frontend" cmd /k "cd Frontend && echo Frontend: http://localhost:4200 && npm start"

echo.
echo Terminais abertos!
echo.

pause
