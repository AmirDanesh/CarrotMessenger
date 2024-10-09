#!/bin/bash

# List all interface files
interfaces=$(find . -name '*.cs' -exec grep -l 'interface' {} \;)

# Temporary file to store interfaces without implementations
no_impl="code-analysis-results/interfaces_without_implementation.txt"
: > $no_impl

for interface in $interfaces; do
  interface_name=$(grep -Po '(?<=interface )\w+' "$interface")
  # Search for implementations
  impl=$(grep -r "class .*:.*$interface_name" . || true)
  if [ -z "$impl" ]; then
    echo "$interface_name" >> $no_impl
  fi
done

if [ -s $no_impl ]; then
  echo "Interfaces without implementations found:"
  cat $no_impl
  exit 1
else
  echo "All interfaces have at least one implementation."
fi
