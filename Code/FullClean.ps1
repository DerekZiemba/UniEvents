Get-ChildItem .\ -include "bin","obj" -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

Get-ChildItem -Directory -Filter .vs -Hidden | foreach ($_) { remove-item $_.fullname -Force -Recurse }