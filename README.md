# BITIRP - Efficient Time Intervals-Related Pattern Mining
## Introduction
**BITIRP**, a novel efficient algorithm for complete frequent TIRP mining. The algorithm starts with $1$-sized TIRPs and grows the TIRPs incrementally. In each step, prefixes of the current frequent TIRPs are identified, as well as suffixes, and their join creates new candidates without generation, but in a data-driven way during the support counting. Indexing the prefixes to all their extended TIRPs prevents the counting of repetitiveness support throughout the mining process. Through rigorous evaluation on six datasets, including run-time comparisons and memory consumption with state-of-the-art TIRP mining methods, **BITIRP** is faster and without compromising on memory consumption.

## Repository Contents
The contents of this repository are as follows:
- The implementation of the **BITIRP** algorithm, implemented in C#.
- All the time intervals datasets on which the method performance has been evaluated in the paper.
- `.csv` files containing the results of the runtime and memory analysis presented in the paper.

## Code
The implementation of the **BITIRP** algorithm is available in the `BITIRP` directory.

## Datasets
The datasets used as part of the evaluation presented in the paper are available in the `Datasets` directory.
For each dataset there is a `.csv` file containing the time intervals, as well as a summarized information regarding the dataset in a `.txt` file.

## Dependencies
- Visual Studio 2022
- .NET 6.0 (LTS) Framework

## Running Instructions
1. Download the repository and install the dependencies.
2. Open the algorithm's project in Visual Studio.
3. Set the algorithm's execution parameters within the `Program.cs` file to the desired values (the set of values used in the paper are supplied).
4. Run.

**Note**: You may need to adjust the values of `dbPathTemplate` and `OUTPUT_PATH` in `Program.cs`.
