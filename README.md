![License](https://img.shields.io/github/license/squideyes/DukasFetch)

**DukasFetch** is a rather basic downloader for DukasFetch.  The code has been  open-sourced under the rather permissive terms of a standard MIT license (see License.md for further details) but, even so, the code is mostly for the author's own personal use, so there is no detailed documentation on offer, nor is there any intent to document the code in the near future.

NOTE: The software takes a dependency on  SquidEyes' <a href="https://github.com/squideyes/Trading" target="_blank">Trading</a> library, which among other limitations means that it is restricted to downloading tick-data for only nine of the major FX pairs.  If you'd like download tick-data for more pairs, please fork both DukasFetch and the Trading library and then update the code to support your own needs.

To run the program, download and compile the source code, then issue a command similar to:

**DukasFetch --folder=C:\TickData --symbols=EURUSD USDJPY --minyear=2020 --replace=true**

|Parameter|Required|Example|Notes|
|---|---|---|---|
|--folder|Yes|C:\TickData|The "base" folder to save tick-data files to.|
|--symbols|No|EURUSD&nbsp;USDJPY|A space-separated list of FX symbols to download tick-data files for.  If omitted, AUDUSD, EURGBP, EURJPY, EURUSD , GBPUSD, NZDUSD, USDCAD, USDCHF and USDJPY ticks will be downloaded.|
|--replace|No|Yes|If true, existing .CSV and .STS tick-data files will be replaced|
|--minyear|No|2020|The minimum year to download tick-data for.  Must be greater than or equal to 2018.

#
Contributions are always welcome (see [CONTRIBUTING.md](https://github.com/squideyes/DukasFetch/blob/master/CONTRIBUTING.md) for details)

**Super-Duper Extra-Important Caveat**:  THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

More to the point, your use of this code may (literally!) lead to your losing thousands of dollars and more. Be careful, and remember: Caveat Emptor!!




