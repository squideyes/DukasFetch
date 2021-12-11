Although **DukasFetch** is mostly for the author's own purposes, it should be fairly easy to update, if desired.  Just note that a few simple rules will need to be followed when submitting a pull-request:

* Be sure to include a copy of the standard license header in each new source code file you create
  * This can be most conveniently done via the SetCodeHeaders extension (see <a href="https://github.com/squideyes/SetCodeHeaders" target="_blank">https://github.com/squideyes/SetCodeHeaders</a> for details).  As an alternative, you can  manually copy an (unchanged!) license header from an existing source code file.
* No changes to the .editorconfig file will be accepted
* No changes to the DukasFetch.sln.headertext file will be accepted
* Pull-requests will not be accepted without a full-coverage unit-test for new or updated functionality
 
NOTE: DukasFetch takes a key dependency on SquidEyes' **Trading** library (<a href="https://github.com/squideyes/Trading" target="_blank">https://github.com/squideyes/Trading</a>), which among other limitations means that the downloader only works with "major" Forex pairs (AUDUSD, EURUSD, EURGBP, EURJPY, GBPUSD, NZDUSD, USDCAD, USDCHF and USDJPY). 