#!/bin/bash

echo "[1/3] Iniciando OrderAccumulator (background)..."
cd Backend/OrderManager/OrderAccumulator && dotnet run &
ACCUMULATOR_PID=$!
cd ..

sleep 2

echo "[2/3] Iniciando OrderGenerator (background)..."
cd Backend/OrderManager/OrderGenerator && dotnet run &
GENERATOR_PID=$!
cd ..

sleep 2

echo "[3/3] Iniciando Frontend (background)..."
cd Frontend && npm start &
FRONTEND_PID=$!
cd ..

echo ""
echo "Processos rodando em background!"
echo "PIDs: Accumulator=$ACCUMULATOR_PID, Generator=$GENERATOR_PID, Frontend=$FRONTEND_PID"
echo "Pressione Ctrl+C para matar todos"

# Aguarda sinais (Ctrl+C)
trap "kill $ACCUMULATOR_PID $GENERATOR_PID $FRONTEND_PID; exit" INT TERM
wait