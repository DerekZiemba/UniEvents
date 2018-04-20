# UniEvents

Graders: to access the system, just use any modern web browser to visit http://unievents.site

Code structure:

* To view code, open UniEvents/UniEvents.sln in the latest version of Visual Studio and use the Solution Explorer to navigate to and open the files you'd like to see.

* "UniEvents/Code/UniEvents.Core" is the location of database models and managers.

* "UniEvents/Code/UniEvents.TSQL" is the location of database initialization files and stored procedures.

* "UniEvents/Code/UniEvents.WebApp/Pages" is the location of .cshtml files for each of the webpages.

* "UniEvents/Code/UniEvents.WebApp/WebAPI/Controllers" is the location of the database controllers.

-----
For development use:

Provided my virtual machine is running and my IP hasn't changed, you can access the site by adding the following to your hosts file. 
Each site corresponds to development branches. 

    108.167.59.103 	unievents.com
    108.167.59.103 	stg.unievents.com
    108.167.59.103 	dev.unievents.com

You may also access directly by IP address:

    http://108.167.59.103:1331/  --master branch
    http://108.167.59.103:1332/  --stg branch
    http://108.167.59.103:1333/  --dev branch