# WebStore — ZPI, sprint 1

Implementacja US-01–US-12 z zaakceptowanej wersji v2 pliku [user-stories.md](dokumentacja/user-stories.md), na podstawie `wymagania/ZPI_NS_Lab1.pdf`. Lider zaakceptował poprawiony kod; ponowna recenzja zakończyła się wynikiem pozytywnym. R-01 i R-02 zamknięto po weryfikacji — szczegóły w [raporcie recenzji](dokumentacja/recenzja.md). **Sprint 1 zamknięty decyzją Lidera „Zamykam sprint 1” z 24.10.2026.**

## Wymagania

- Windows, SDK .NET 10.0.402 (wersja przypięta w `global.json`).
- SQL Server LocalDB, instancja MSSQLLocalDB. Domyślna baza projektu: **ZPI_WebStore_Sprint1**.
- Node.js zgodny z Vite 8 (sprawdzono na Node 24.15.0), npm (sprawdzono 11.12.1).
- Git oraz VS Code z C#, SQL Server (mssql) i Git Graph. Zalecenia dodatków znajdują się w `.vscode/extensions.json`.
- Odpowiednia licencja AutoMapper 16.2.0, którą zgodnie z decyzją Lidera zapewnia Lider. [Warunki producenta](https://automapper.io/), [konfiguracja licencji](https://docs.automapper.io/en/stable/License-configuration.html).

## Przygotowanie

Polecenia uruchamiaj w PowerShell w katalogu repozytorium:

```powershell
cd C:\Users\janus\ZPI
dotnet --version
node --version
npm --version
SqlLocalDB info MSSQLLocalDB
```

Jeśli instancji nie ma: `SqlLocalDB create MSSQLLocalDB -s`. Jeśli istnieje i jest zatrzymana: `SqlLocalDB start MSSQLLocalDB`.

```powershell
dotnet restore --locked-mode
dotnet tool restore
npm --prefix WebStore.Web/ClientApp ci
dotnet ef database update --project WebStore.DAL --startup-project WebStore.DAL
dotnet build WebStore.sln --no-restore
```

Migracje są w WebStore.DAL/EF/Migrations. Ich zastosowanie tworzy bazę i schemat; aplikacja nie modyfikuje schematu samoczynnie przy starcie. Nie używaj `EnsureCreated` na bazie projektu zarządzanej migracjami.

Domyślne połączenie używa lokalnego konta Windows. Dla innej lokalnej instancji ustaw `ConnectionStrings__WebStore` przed poleceniami migracji i uruchomienia. Fabryka DAL i aplikacja respektują tę samą zmienną. Nie umieszczaj haseł w plikach śledzonych przez Git.

AutoMapper odczytuje `AUTOMAPPER_LICENSE_KEY` z otoczenia. Ustaw klucz lokalnie przed uruchomieniem serwera lub testów; nie przekazuj go w rozmowie ani w repozytorium. Serwer obsługuje również konfigurację `AutoMapper:LicenseKey`. Brak klucza powoduje ostrzeżenie biblioteki; nie zostało ono wyciszone. Dostarczenie klucza pozostaje po stronie Lidera.

## Uruchomienie

```powershell
dotnet run --project WebStore.Web
```

Przed ponownym budowaniem w tej samej konfiguracji zatrzymaj poprzednio uruchomiony serwer, aby zwolnić jego pliki.

Otwórz [http://localhost:5080](http://localhost:5080). Budowanie przygotowuje klienta React w `wwwroot`, a ASP.NET Core udostępnia stronę i `/api/status` z jednego adresu. Strona potwierdza odpowiedź serwera; endpoint statusu nie deklaruje dostępności bazy danych.

Zakres sprintu obejmuje fundament aplikacji i autonomiczne usługi. Nie dodano ekranów zakupów, panelu administracyjnego ani endpointów biznesowych, których nie zawierają zaakceptowane stories. Baza projektu startuje bez przykładowych kont i danych biznesowych; testy tworzą własne dane.

Przy pracy nad samym klientem można uruchomić serwer oraz, w drugim terminalu, `npm --prefix WebStore.Web/ClientApp run dev`. Vite przekazuje `/api` do portu 5080.

## Projekty i reguły

| Projekt | Odpowiedzialność |
| --- | --- |
| WebStore.Model | Model sklepu i użytkowników Identity z kluczem int |
| WebStore.DAL | EF Core 10, SQL Server, relacje TPH i migracje |
| WebStore.ViewModels | Dane wejściowe i wynikowe usług |
| WebStore.Services | ProductService, OrderService, InvoiceService, StoreService, AddressService i AutoMapper |
| WebStore.Web | Serwer ASP.NET Core oraz klient React w ClientApp |
| WebStore.Tests | Testy xUnit z EF InMemory i test migracji na SQL Server |

Usługi są rejestrowane przez `AddWebStoreServices`. Wstrzykuj interfejs usługi w zakresie DI z `ApplicationDbContext`. Przed zapisem przez usługę kontekst nie może mieć cudzych niezapisanych zmian. Relacyjne operacje zapisu korzystają z transakcji Serializable. Po sprawdzeniu braku niezapisanych zmian usługa odłącza wcześniej śledzone encje, aby walidować bieżący stan bazy. Nie należy polegać na dalszym śledzeniu wcześniej pobranych obiektów po wywołaniu zapisu usługi. Błąd wycofuje zapis, a kontekst odrzuca zmiany śledzone w nieudanej operacji. Wymagane pola pracownika są sprawdzane zarówno przez UserManager (IdentityResult z błędami), jak i przez SaveChanges/SaveChangesAsync (ValidationException).

Domyślna faktura grupuje wiele zamówień jednego klienta. Cena pozycji zamówienia jest historyczna i nie zmienia się wraz z katalogiem; zamówienie z fakturą nie podlega edycji. Zapis zamówienia nie rezerwuje i nie odejmuje stanów. Adresy mają dokładnie jednego właściciela, a klient wybiera po jednym adresie do rozliczeń i dostawy. Pełne kontrakty i wyjątki opisuje zaakceptowana specyfikacja.

Numer faktury jest ograniczony do 450 znaków ze względu na indeks SQL Server, bez rozróżniania wielkości liter. Kwoty mają typ SQL decimal(18,2). Pola tekstowe produktu nie mają dodatkowych limitów aplikacyjnych. Świadomie przypięto System.Security.Cryptography.Pkcs 10.0.13 w DAL i Web, aby zastąpić podatną zależność pośrednią narzędzi EF. Wersje bibliotek utrwalają pliki csproj, packages.lock.json i package-lock.json.

## Sprawdzanie

```powershell
dotnet test WebStore.sln --no-restore
dotnet test WebStore.Tests --filter 'Category!=SqlServer'
dotnet test WebStore.Tests --filter 'Category=SqlServer'
dotnet test WebStore.Tests --filter ProductServiceUnitTests
dotnet test WebStore.Tests --filter UpdatesExistingProductAndKeepsStock
dotnet ef migrations has-pending-model-changes --project WebStore.DAL --startup-project WebStore.DAL
npm --prefix WebStore.Web/ClientApp audit
```

Test SQL Server wymaga działającego LocalDB. Tworzy oddzielną bazę o nazwie `ZPI_WebStore_Test_<losowy identyfikator>`, stosuje migracje i usuwa wyłącznie tę bazę po teście. Nie korzysta z bazy ZPI_WebStore_Sprint1. Pozostałe testy tworzą odrębne bazy InMemory dla każdego przypadku. InMemory nie zastępuje kontroli kluczy i ograniczeń SQL Server.

```powershell
dotnet publish WebStore.Web -c Release -o output/publish
```

Publikacja obejmuje serwer i zbudowane pliki React. Katalog publikacji jest wynikiem budowania i nie podlega commitowi. Przedstawienie kodu Liderowi nie oznacza akceptacji kodu ani zakończenia recenzji. Agenci nie wykonują commitów.

## Wynik sprawdzenia sprintu 1

Po poprawkach programisty R-01 i R-02: **66/66 testów zakończonych powodzeniem**, w tym 18 nowych przypadków regresyjnych i sprawdzenia na rzeczywistym SQL Server LocalDB. Pierwsze uruchomienie dwóch podstawowych nowych testów na starej implementacji potwierdziło oba błędy; po poprawkach cały zestaw przeszedł. Publikacja Release z klientem zakończyła się powodzeniem. Poprawki nie zmieniają schematu, co potwierdziło polecenie kontroli modelu — nie trzeba stosować nowej migracji.

Po ponownej akceptacji kodu przez Lidera recenzent niezależnie potwierdził **66/66 testów**, budowanie Release bez błędów i ostrzeżeń oraz brak zmian wymagających migracji. Aktualny [raport](dokumentacja/recenzja.md) ma wynik pozytywny; oba zgłoszenia zamknięto. Recenzent nie zmieniał implementacji.

Wcześniejszy test przeglądarkowy potwierdził komunikację React–serwer, widok komputera i telefonu oraz ponowienie po błędzie; poprzednie audyty npm i .NET nie wykazały znanych podatności. Po poprawkach nie zmieniono klienta ani zależności i nie powtarzano tych sprawdzeń.

Klucz licencji AutoMapper pozostaje poza repozytorium i konfiguruje go Lider. Na odrębne polecenia Lidera utworzono commity `a10dd36` (wymagania i ustalenia) oraz `b805d8f` (implementacja, testy i recenzja), a następnie wysłano je do `origin/lab1`.
