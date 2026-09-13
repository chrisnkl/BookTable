# BookTable

Για να τρέξει σωστά η εφαρμογή, είναι απαραίτηση η χρήση SQL Server και Azurite.
Για να τρέξει το frontend είναι αναγκαίο το Node.

## BookTable.BookTable
1. Εισαγωγή connection path στο αρχείο appsettings.json, στο πεδίο "DefaultConnection".
2. Κλήση των ακόλουθων εντολών στη κονσόλα:
<ul>
    <li>dotnet ef migrations add InitialCreation (SKIP αν ο φάκελος Migrations δεν είναι κενός)</li>
    <li>dotnet ef database update</li>
</ul>
3. Εκτέλεση BookTable.BookTable

## BookTable.NotificationService
1. Εκτέλεση BookTable.NotificationService

## Frontend
1. Κλήση των ακόλουθων εντολών στη κονσόλα:
<ul>
    <li>npm install</li>
    <li>npm run dev</li>
</ul>

## Azurite
Σημαντικό είναι να γίνει εκκίνηση του Azurite, ώστε να τρέξει σωστά η εφαρμογή.
Άνοιγμα CMD και εκτέλεση ακόλουθης εντολής: 
<ul>
    <li>azurite</li>
</ul>

Αν δεν υπάρχει azurite στο σύστημα, μπορεί να εγκατασταθεί μέσω NPM με την ακόλουθη εντολή:
<ul>
    <li>npm install -g azurite</li>
</ul>

Αν το azurite τρέξει σε άλλο location (θα φανεί στη κονσόλα), στο frontend υπαρχει .env αρχείο και βάζετε το runtime location στο env variable `VITE_BLOB_STORAGE_URL`

Τα projects, αν χρειαστεί, υπάρχουν και αναρτημένα στο github εδώ:
<ul>
    <li>Frontend: https://github.com/chrisnkl/booktable-frontend</li>
    <li>Backend: https://github.com/chrisnkl/BookTable</li>
</ul>