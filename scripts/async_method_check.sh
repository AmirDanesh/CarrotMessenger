#!/bin/bash

# Install necessary tools
dotnet tool install --global dotnet-code-analyzer

# Run the analyzer
dotnet code-analyzer async-method-name --exclude-external --output code-analysis-results/async_methods.txt

# Check if any violations are found
if [ -s code-analysis-results/async_methods.txt ]; then
  echo "Async method naming violations found:"
  cat code-analysis-results/async_methods.txt
  exit 1
else
  echo "No async method naming violations found."
fi
