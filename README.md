# WebApplicatie
Webapplicatie opdracht die hoort bij het semester Web Development op Windesheim. Ik heb en hekel aan web development en ik hoop dat ik er nooit van mijn leven nog iets mee hoef te doen.

## Uitvoeren in development op Windows
Om migraties uit te voeren, run deze commands in de package manager console.
(View > Other Windows > Package Manager Console)
`> update-database -Context SwapGame_DbContext`
`> update-database -Context SwapGame_IdentityDbContext`

## Deployen in Test
apache 2 installeren
`apt install apache2`

Kijk in eigen browser even of http://192.168.56.102/ bestaat (dit is de url die mijn Debian VM blootstelt).
Als je "Service Unavailable" ziet dan werkt Apache in ieder geval.

Pas de proxy instellingen aan om requests te redirecten naar de projecten die we zometeen gaan aanzetten.

`sudo nano /etc/apache2/sites-available/000-default.conf`
Voeg dan deze regels toe tussen de <VirtualHost> tags.
```
ProxyPass /api http://localhost:6009/api
ProxyPassReverse /api http://localhost:6009/api
ProxyPass / http://localhost:6010/
ProxyPassReverse / http://localhost:6010/
```

Onthoud dat 6010 is bedoeld voor MVC, en 6009 voor de API. 

Daarna:
`sudo systemctl restart apache2`

Dit zorgt ervoor dat apache deze instellingen laadt.

Clone de repo naar een folder. 
`git clone https://github.com/SanderWassenberg/WebApplicatie.git`
of update naar de recentste versie met `git pull`. 

Configureer het project: Verander de waardes in `SwapGame_API/appsettings.json` overal waar "<MISSING>" staat.
De database connectionstring is in het volgende format:
`Server=127.0.0.1;Database=SwapGameDB;TrustServerCertificate=True;Encrypt=True;User Id=SA;Password=VUL_HIER_HET_WACHTWOORD_IN`

`cd` naar `SwapGame/SwapGame_API/` voer daarna de migrations uit:
`dotnet-ef database update --context SwapGame_DbContext`
`dotnet-ef database update --context SwapGame_IdentityDbContext`

`cd` terug naar `SwapGame/`. Nu kunnen we de projecten runnen. Je kan hiervoor het beste twee terminal tabbaden openen.
`dotnet run environment=Test --project SwapGame_API --urls=http://localhost:6009`
`dotnet run environment=Test --project SwapGame_MVC --urls=http://localhost:6010`

## Deployen in Acceptatie
Op de Test host, `cd` naar `Swapgame/` en run de volgende commando's om het project te compileren:

`dotnet publish SwapGame_API -r linux-x64 --self-contained false --configuration Release`
`dotnet publish SwapGame_MVC -r linux-x64 --self-contained false --configuration Release`

`cd` naar `SwapGame/SwapGame_API/` en run de volgende commando's om de migration scripts te genereren:

`mkdir sql_scripts`
`dotnet ef migrations script --context SwapGame_DbContext -o sql_scripts/init_db.sql`
`dotnet ef migrations script --context SwapGame_IdentityDbContext -o sql_scripts/init_idb.sql`

Let er op dat je hier twee verschillende bestanden van maakt, `init_db.sql` en `init_idb.sql`

Open `init_db.sql` en voeg aan de top het volgende toe:
```
DROP DATABASE IF EXISTS SwapGame_DB
GO
CREATE DATABASE SwapGame_DB
GO
USE SwapGame_DB
GO
```

Voeg `USE SwapGame_DB` en `GO` ook toe aan `init_idb.sql`.

ssh naar de Skylab Debian machine en zorg ervoor dat de server niet meer aanstaat.
`ps -u student -f`
als je processen van MVC en API aanstaan kan je deze stoppen met `kill <PID van api of mvc process>`

Als de processen uit staan ga je weer terug naar de testomgeving.

Kopieer alle bestanden naar de Skylab Debian machine.
`scp -r SwapGame_API/bin/Release/net7.0/linux-x64/publish/ student@145.44.232.24:showcase/api`
`scp -r SwapGame_MVC/bin/Release/net7.0/linux-x64/publish/ student@145.44.232.24:showcase/mvc`
`scp -r SwapGame_API/sql_scripts/ student@145.44.232.24:showcase`

ssh naar de Skylab Debian machine
Kijk of de SQL server actief is met `docker ps -a`

Kopieer de scripts naar de sql container
`docker cp sql_scripts/init_db.sql sql1:/`
`docker cp sql_scripts/init_idb.sql sql1:/`

Voer de scripts uit in de container (in de goede volgorde, omdat we het weggooien en creëren van de database maar in één bestand doen.)
`docker exec sql1 /bin/sh -c '/opt/mssql-tools/bin/sqlcmd -I -S localhost -U SA -P "SA_WACHTWOORD_HIER" -i /init_db.sql'`
`docker exec sql1 /bin/sh -c '/opt/mssql-tools/bin/sqlcmd -I -S localhost -U SA -P "SA_WACHTWOORD_HIER" -i /init_idb.sql'`

Run de gepubliceerde bestanden. Het is belangrijk dat je dit doet vanuit de goede folders zodat 
de current working directory van de processen in die folder is en ze de benodigde bestanden kunnen vinden 
(mvc moet de wwwroot folder bijv. kunnen vinden).
`cd ~/showcase/api/publish`
`dotnet SwapGame_API.dll --urls=http://localhost:6009 &`
`cd ~/showcase/mvc/publish`
`dotnet SwapGame_MVC.dll --urls=http://localhost:6010 &`

De ` &` aan het einde van de commands zorgt ervoor dat deze commands als een achtergrondproces worden gestart.
Als het goed is moet je nu de site kunnen bekijken op sanderwassenberg.hbo-ict.org




