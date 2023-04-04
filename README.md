# WebApplicatie
Webapplicatie opdracht die hoort bij het semester Web Development op Windesheim. Ik heb en hekel aan web development en ik hoop dat ik er nooit van mijn leven nog iets mee hoef te doen.

## Stappen om uit te voeren bij het opzetten van deze applicatie op een nieuwe machine

### op linux in acceptatie
in de commandline in de path van het SPI project
`dotnet-ef database update`

verander de waardes in appsettings.json overal waar "<MISSING>" staat.
De database connectionstring is in het volgende format:
`Server=127.0.0.1;Database=SwapGameDB;TrustServerCertificate=True;Encrypt=True;User Id=SA;Password=VUL_HIER_HET_WACHTWOORD_IN`

### op windows in development
in de package manager console
`> update-database`