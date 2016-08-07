---
title: PDB molecular viewer
tags: [PDB, GCModeller, 3d, gdi+]
date: 2016-8-6
---


Reading the pdb model file:

```vbnet
Imports SMRUCC.genomics.Data.RCSB.PDB

Dim pdb As PDB = PDB.Load(path)
```