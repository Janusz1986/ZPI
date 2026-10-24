# Recenzja sprintu 1 — wynik pozytywny

## Zakres i akceptacja

- Źródło: `wymagania/ZPI_NS_Lab1.pdf`, 21 stron.
- Podstawa: zaakceptowane US-01–US-12 i P-01–P-04, wersja v2 `dokumentacja/user-stories.md`.
- Lider zaakceptował pierwotny kod wiadomością „Tak, akceptuję kod — rozpocznij recenzję”. Pierwsza recenzja wykryła R-01 i R-02; na polecenie Lidera programista przygotował poprawki.
- **Akceptacja poprawek i uruchomienie ponownej recenzji:** odpowiedź Lidera „Tak, akceptuję” na pytanie o akceptację poprawionego kodu i rozpoczęcie ponownej recenzji, 24.10.2026, rozmowa projektu ZPI.
- Data ponownej recenzji: 24.10.2026, Europe/Warsaw.
- Sprawdzony stan: lokalne pliki robocze bez commita. SHA-256 manifestu plików sześciu projektów, z pominięciem bin, obj, node_modules i wwwroot: `3e97ced3c4cc46e49da7d7070210da2a7a0f3a05b61e266e09aee678c579fb75`. Zgodność z pakietem przedstawionym Liderowi potwierdzono przed i po sprawdzeniach.
- Środowisko: Windows, SDK .NET 10.0.402, Node 24.15.0, SQL Server LocalDB 15.0.4382.1; EF Core 10.0.13, AutoMapper 16.2.0, React 19.3.1 i Vite 8.3.5.
- Recenzent nie zmieniał implementacji ani testów projektu. Zaktualizował tylko raport i status w README.

## Wyniki ponownej weryfikacji

1. Przegląd poprawek w BaseService, StationaryStoreEmployee, ApplicationDbContext, EmployeeFieldsValidator oraz rejestracji Identity. Sprawdzono także nowe testy i zgodność z dotychczasowymi kontraktami usług.
2. Recenzent wykonał `dotnet test WebStore.Tests -c Release --no-restore`: **66/66 testów zakończonych powodzeniem, 0 pominiętych**. Zestaw zawiera 48 wcześniejszych i 18 nowych przypadków regresyjnych.
3. Wykonano `dotnet build WebStore.sln -c Release --no-restore`: **0 błędów, 0 ostrzeżeń**, łącznie z budowaniem klienta React.
4. Kontrola `ef migrations has-pending-model-changes` dla Release potwierdziła brak zmian modelu wymagających migracji.
5. Testy relacyjne korzystały z osobnych, losowo nazwanych baz `ZPI_WebStore_Test_<identyfikator>`, stosowały migracje, sprawdzały stan przez nowe konteksty i usuwały swoje bazy po zakończeniu. Nie zmieniano danych biznesowych bazy projektu.

## R-01 — zamknięty po ponownej weryfikacji

**Waga zgłoszenia:** istotny, P1. **Wymagania:** US-09/2–3, US-10/2, P-03; pokrycie testowe US-08/5.

Poprzednio wcześniejszy zakres DI A pozwalał zmienić kwotę zamówienia z 2000 do 3000 po wystawieniu faktury przez zakres B, pozostawiając fakturę na 2000. Przyczyną była walidacja zapamiętanego InvoiceId zamiast aktualnego stanu bazy.

BaseService teraz najpierw odrzuca cudze niezapisane zmiany, a następnie odłącza stare śledzone encje przed pobraniem danych w transakcji Serializable. Bieżące reguły nie opierają się na encjach z wcześniejszej operacji.

**Dowody:**
- RejectsOrderUpdateAfterAnotherScopeIssuesInvoice odtwarza pierwotny scenariusz z dwoma zakresami DI na SQL Server. Zapis zgłasza InvalidOperationException, ilość pozostaje 2, kwoty zamówienia i faktury pozostają równe 2000.
- InvoiceUsesAmountUpdatedInAnotherScope potwierdza, że faktura korzysta z aktualnej kwoty zamówienia 3000 po zmianie w innym zakresie.
- CannotMoveOrderToAnotherInvoiceUsingStaleContext odrzuca próbę przeniesienia zafakturowanego zamówienia; baza zachowuje jedną fakturę i pierwotne powiązanie.
- AddressValidationUsesCurrentDefaultSelection (2 przypadki) potwierdza uwzględnienie aktualnego wyboru adresu z innego zakresu.
- PendingCallerChangesAreNotDiscardedByService potwierdza, że odmowa operacji nie usuwa niezapisanych zmian wywołującego.

**Status:** poprawka potwierdzona przez recenzenta; brak odtworzenia błędu w sprawdzonych scenariuszach.

## R-02 — zamknięty po ponownej weryfikacji

**Waga zgłoszenia:** istotny, P2. **Wymagania:** US-04/5, US-03/5, P-02; pokrycie US-08/5.

Poprzednio UserManager przyjmował pracownika z pustymi FirstName, LastName i Position. IsRequired chroniło jedynie przed null.

Reguły niepustych pól są teraz wspólne w walidacji domenowej StationaryStoreEmployee. EmployeeFieldsValidator przekazuje je jako błędy Identity przy tworzeniu i aktualizacji. ApplicationDbContext sprawdza je również przed SaveChanges i SaveChangesAsync.

**Dowody:**
- IdentityRejectsEmptyEmployeeFieldsOnCreateAndUpdate: 9 kombinacji trzech pól i wartości null, pustej oraz białych znaków. Odrzucane są zarówno utworzenie, jak i aktualizacja; dane odczytane z nowego zakresu pozostają prawidłowe.
- DirectContextSaveAlsoRejectsInvalidEmployee: zapis synchroniczny i asynchroniczny odrzucają nieprawidłowego pracownika przez ValidationException.
- EmployeeValidationAndValidUpdatesPersistOnSqlServer potwierdza odrzucenie pierwotnego przypadku, poprawny zapis i aktualizację prawidłowego pracownika oraz brak utrwalenia błędnej aktualizacji. Bezpośredni błędny zapis w SQL Server również jest odrzucany.
- Dotychczasowy test modelu i relacji z pracownikiem nadal przechodzi.

**Status:** poprawka potwierdzona przez recenzenta; brak odtworzenia błędu w sprawdzonych scenariuszach.

## Pokrycie kryteriów akceptacji

Macierz przedstawia bieżący stan. Niezmienione kryteria zachowują dowody z pierwszej recenzji, uzupełnione ponownym uruchomieniem wszystkich testów i budowaniem Release. Kryteria dotknięte R-01 i R-02 sprawdzono ponownie. Status „spełnione” odnosi się do wskazanego scenariusza i przeglądu; nie jest gwarancją braku wszelkich możliwych błędów.

| Story | Kryterium | Wynik | Sprawdzenie / dowód |
| --- | --- | --- | --- |
| US-01 | 1 | spełnione | SDK 10.0.402: odczyt wersji; global.json wskazuje tę wersję. |
| US-01 | 2 | spełnione | Instancja MSSQLLocalDB działa; niezależne testy migracji na SQL Server powiodły się. |
| US-01 | 3 | spełnione | Potwierdzono instalacje ms-dotnettools.csharp 2.160.4, ms-mssql.mssql 1.47.0, mhutchie.git-graph 1.30.0 oraz VS Code. |
| US-01 | 4 | spełnione | Git rozpoznaje ZPI; konfiguracja repozytorium nie została zmieniona podczas recenzji. |
| US-01 | 5 | spełnione | Node 24.15.0; klient React zbudowany przez Vite 8.3.5. |
| US-02 | 1 | spełnione | WebStore.sln i pięć projektów aplikacji; osobny WebStore.Tests. |
| US-02 | 2 | spełnione | Przegląd ProjectReference w sześciu plikach csproj. |
| US-02 | 3 | spełnione | ASP.NET Core net10.0; React w WebStore.Web/ClientApp. |
| US-02 | 4 | spełnione | Build Release ponownie: 0 błędów i ostrzeżeń. Wcześniejszy test przeglądarkowy potwierdził stronę i /api/status; klient nie został zmieniony. |
| US-02 | 5 | spełnione | Przegląd README: przygotowanie, migracja, uruchomienie, testy i publikacja. |
| US-03 | 1 | spełnione | Przegląd User, Customer, Supplier; testy Identity i TPH. |
| US-03 | 2 | spełnione | Przegląd pól i typów Order oraz migracji. |
| US-03 | 3 | spełnione | Przegląd pól i typów Product oraz relacji Category. |
| US-03 | 4 | spełnione | Przegląd ProductStock i OrderProduct; test relacji w SQL Server. |
| US-03 | 5 | spełnione | Rozszerzenia modelu i relacje sprawdzone; walidacja wymaganych pól pracownika potwierdzona w testach Identity i DAL. R-02 zamknięty. |
| US-04 | 1 | spełnione | AddressServiceUnitTests i SQL Server: adresy obiektowe, wybór obu ról, przełączenie wyboru. |
| US-04 | 2 | spełnione | Model StationaryStoreEmployee dziedziczy po User; relacja ze sklepem działa w SQL Server. |
| US-04 | 3 | spełnione | Category ma Name i Tag; Product ma CategoryId i powiązanie. |
| US-04 | 4 | spełnione | Invoice oraz Order.InvoiceId; SQL Server potwierdza wiele zamówień na fakturę. |
| US-04 | 5 | spełnione | IValidatableObject, EmployeeFieldsValidator oraz SaveChanges/SaveChangesAsync odrzucają puste wymagane pola; prawidłowy pracownik i jego aktualizacja są utrwalane. R-02 zamknięty. |
| US-05 | 1 | spełnione | ApplicationDbContext, provider SQL Server i niezależny test relacyjny. |
| US-05 | 2 | spełnione | Przegląd Fluent API i migracji; test danych oraz kluczy obcych. |
| US-05 | 3 | spełnione | Przegląd FK; SQL: adresy klienta i dwa adresy sklepu, pracownik sklepu, dwa zamówienia faktury. |
| US-05 | 4 | spełnione | Klucz złożony OrderId + ProductId; test zapisu Quantity i odrzucenia Quantity=0. |
| US-05 | 5 | spełnione | Discriminator UserType; po ponownym otwarciu kontekstu odczytano Customer, Supplier i Employee. |
| US-05 | 6 | spełnione | Test SQL stosuje migrację i odczytuje utrwalone dane przez nowy kontekst. Ponowna kontrola modelu: brak oczekujących zmian; schemat nie zmienił się od wcześniejszej recenzji. |
| US-05 | 7 | spełnione | README i fabryka kontekstu: konfiguracja połączenia i stosowanie migracji bez usuwania bazy projektu. |
| US-06 | 1 | spełnione | Przegląd Interfaces i ConcreteServices, pięć publicznych interfejsów I…Service. |
| US-06 | 2 | spełnione | BaseService: kontekst, mapper i logger przekazywane konstruktorem. |
| US-06 | 3 | spełnione | Przegląd klas WebStore.ViewModels/VM; sufiks Vm. |
| US-06 | 4 | spełnione | ContractTests.MappingAndIdentityAreConfigured: AssertConfigurationIsValid; testy wyników usług. |
| US-06 | 5 | spełnione | Walidacja kontenera DI w TestHost i utworzenie wszystkich pięciu usług. |
| US-07 | 1 | spełnione | Przegląd kontraktu i dziedziczenia ProductService; 13/13 przypadków klasy testowej. |
| US-07 | 2 | spełnione | Przegląd pól, typów i Required w AddOrUpdateProductVm. |
| US-07 | 3 | spełnione | Testy zapisu, aktualizacji, null/0/ujemnego/nieistniejącego Id, brakujących referencji i walidacji. |
| US-07 | 4 | spełnione | Testy jednego wyniku, braku wyniku, pustej listy, filtra oraz wszystkich produktów. |
| US-07 | 5 | spełnione | Testy ArgumentNullException; przegląd logowania i ponownego zgłaszania wyjątków w BaseService. |
| US-07 | 6 | spełnione | Test sumy stanów 3+4=7, braku stanów=0 i zachowania stanów przy aktualizacji. |
| US-08 | 1 | spełnione | Przegląd csproj; uruchomione xUnit, EF InMemory i DI. |
| US-08 | 2 | spełnione | TestHost rejestruje zależności i zapisuje dane przed testem; ContractTests sprawdza mapowanie i Identity. |
| US-08 | 3 | spełnione | 13 przypadków ProductServiceUnitTests: odczyty, dodanie i aktualizacja z kontrolą utrwalonych danych. |
| US-08 | 4 | spełnione | ReadsSingleFilteredAndAllProductsWithStockSum sprawdza liczbę 2, bez tautologii. |
| US-08 | 5 | spełnione | ReviewRegressionTests: 18 nowych przypadków pokrywa oba zgłoszenia, dwa zakresy DI, kwoty faktur, wybór adresów, niezapisane zmiany i walidację pracowników. Cała suite 66/66. |
| US-08 | 6 | spełnione | Recenzent uruchomił 66/66 testów bez pominięć. Izolacja baz zachowana; wcześniejsze sprawdzenie filtrowania klasy 13/13 i pojedynczego testu 1/1 pozostaje aktualne. |
| US-08 | 7 | spełnione | SqlServerIntegrationTests wykonano na LocalDB; dodatkowe scenariusze recenzji także na SQL Server. |
| US-09 | 1 | spełnione | Przegląd IOrderService, OrderService, BaseService i view modeli. |
| US-09 | 2 | spełnione | RejectsOrderUpdateAfterAnotherScopeIssuesInvoice: edycja po wystawieniu faktury w innym zakresie DI kończy się InvalidOperationException; dane nie zostały zmienione. |
| US-09 | 3 | spełnione | Po odrzuconej edycji kwoty zamówienia i faktury pozostają równe 2000, ilość pozycji pozostaje 2. Pozostałe testy cen historycznych, kwot i magazynu przechodzą. |
| US-09 | 4 | spełnione | OrderServiceUnitTests oraz trwałe testy regresyjne R-01 na SQL Server przechodzą. |
| US-10 | 1 | spełnione | Przegląd IInvoiceService, InvoiceService, BaseService i view modeli. |
| US-10 | 2 | spełnione | InvoiceUsesAmountUpdatedInAnotherScope i CannotMoveOrderToAnotherInvoiceUsingStaleContext potwierdzają bieżącą kwotę i blokadę przeniesienia; pozostałe testy grupowania i numerów przechodzą. |
| US-10 | 3 | spełnione | InvoiceServiceUnitTests przechodzą: grupowanie, zastępowanie powiązań, numer, daty, klient i błędy. |
| US-11 | 1 | spełnione | Przegląd IStoreService, StoreService i view modeli. |
| US-11 | 2 | spełnione | StoreServiceUnitTests i test SQL: aktualizacja danych sklepu zachowuje adresy i pracowników. |
| US-11 | 3 | spełnione | Testy dodania null/0, aktualizacji, filtrów, braku wyniku i walidacji nazwy przechodzą. |
| US-12 | 1 | spełnione | Przegląd IAddressService, AddressService i view modeli. |
| US-12 | 2 | spełnione | Dotychczasowe testy oraz AddressValidationUsesCurrentDefaultSelection dla dwóch wyborów potwierdzają aktualną rolę adresu po zmianie w innym zakresie DI. |
| US-12 | 3 | spełnione | Testy operacji, błędów i utrwalenia adresów przechodzą. |

## Ograniczenia i przekazanie Liderowi

- Nie przeprowadzono pełnych testów obciążeniowych ani wszystkich możliwych przeplotów równoczesnych transakcji. Sprawdzono scenariusze dwóch zakresów DI opisane w zgłoszeniach i regresjach.
- Klient i zależności nie zostały zmienione przez poprawki. W tej ponownej recenzji nie powtarzano testu przeglądarkowego, publikacji ani audytów podatności. Dowody wcześniejszej recenzji UI pozostają aktualne dla niezmienionego klienta; wyników programisty nie opisujemy jako nowych sprawdzeń recenzenta.
- Nie weryfikowano poprawności klucza licencji AutoMapper. Lider zapewnia odpowiednią licencję i konfiguruje klucz lokalnie, poza repozytorium.
- Walidacja kontekstu obejmuje zapisy przez aplikację i Identity; nie jest zabezpieczeniem przed celowym pominięciem aplikacji przez surowy SQL.
- Brak otwartych potwierdzonych defektów w sprawdzonym zakresie. Nie zmieniano zakresu stories, nie wykonywano commitów ani push.

**Wynik końcowy recenzji: pozytywny. R-01 i R-02 zamknięte.** Implementacja sprintu 1 jest gotowa do decyzji Lidera o zamknięciu iteracji. Recenzent nie zamyka sprintu w jego imieniu.
