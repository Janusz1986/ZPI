# Sprint 1 — user stories

- Źródło: `wymagania/ZPI_NS_Lab1.pdf`, 21 stron; numery stron poniżej oznaczają strony pliku PDF.
- Wersja: v2.
- Status: US-01–US-12 w wersji v2 wraz z P-01–P-04 zaakceptowane przez Lidera.
- Status sprintu: zamknięty decyzją Lidera „Zamykam sprint 1” z 24.10.2026, po akceptacji kodu i pozytywnej recenzji; zakres wymagań v2 pozostaje bez zmian.
- Decyzja: odpowiedź Lidera „Tak, akceptuję.” na pytanie o akceptację wszystkich 12 stories v2 i P-01–P-04, w rozmowie projektu 24.10.2026. Akceptacja dotyczy wymagań, nie kodu.
- Zlecenie: Lider polecił rozpocząć pierwszy sprint. Nie stanowi to akceptacji stories ani kodu.

## Cel i zakres

Przygotowanie fundamentu aplikacji WebStore: uruchamialnej solucji klient–serwer, modelu sklepu, dostępu do SQL Server, autonomicznych usług i testów. Dokument opisuje zadania techniczne, dlatego część stories ma aktora technicznego. Nie dopisano procesu zakupowego, logowania ani ekranów zarządzania, których zachowania nie zostały opisane w PDF. Zaakceptowany zakres interfejsu: uruchamialny klient React komunikujący się z serwerem, bez dodatkowych ekranów biznesowych; PDF ich nie specyfikuje. Zakres ten został zaakceptowany wraz z v2.

- Decyzja licencyjna Lidera: „Aktualny AutoMapper — zapewnię odpowiednią licencję” (24.10.2026). Klucz pozostaje poza repozytorium.

## Ustalenia nadrzędne

- Komunikacja i dokumentacja są po polsku.
- Repozytorium i katalog projektu zostały już wskazane przez Lidera: `C:\Users\janus\ZPI`, `https://github.com/Janusz1986/ZPI.git`.
- Polecenia PDF dotyczące commitów i push (s. 6 i 21) wykonuje wyłącznie Lider. Agenci nie commitują i nie wysyłają zmian.
- PDF zaleca fikcyjny e-mail (s. 6), ale Lider podał własną tożsamość Git w rozmowie. Nie zmieniamy jej na podstawie instrukcji laboratoryjnej.
- PDF opisuje gałąź studenta i nazwę katalogu (s. 4–7). Nie zmieniamy istniejącego katalogu ani nie wymyślamy imienia i nazwiska do nazwy gałęzi. Ewentualną wymaganą nazwę gałęzi wskaże Lider przed pracą programisty.

## Decyzja Lidera i zgodność ze źródłem

Lider wybrał najnowszy .NET i zgodne biblioteki oraz zażądał zgodności z PDF. Dla pozostałych niejasności polecił przygotować propozycje. Następnie zaakceptował wszystkie stories v2 wraz z P-01–P-04; implementacja jest autoryzowana.

**P-01 — technologia:** najnowsze stabilne wydanie to .NET 10 LTS: SDK 10.0.402, runtime 10.0.13 (weryfikacja 24.10.2026). Projekty aplikacji i testy mają target net10.0. .NET 11 jest obecnie wydaniem RC. Na komputerze wykryto SDK 10.0.201; aktualizacja SDK będzie częścią przygotowania środowiska po akceptacji. Wersje SDK i runtime mają odrębną numerację.

Źródła: [wydania .NET](https://dotnet.microsoft.com/en-us/download/dotnet), [SDK .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

Zgodność z PDF obejmuje pięć warstw aplikacji i dodatkowy projekt testowy, nazwy klas i usług, React, SQL Server, EF Core, Identity z kluczem int, TPH, wszystkie wymagane relacje, AutoMapper, logowanie i testy xUnit. Wersje .NET oraz bibliotek aktualizujemy zgodnie z decyzją Lidera. Dokładne stabilne wersje zależności programista zweryfikuje i utrwali podczas implementacji; nie zakłada zgodności wyłącznie na podstawie numeru wersji. Jeżeli aktualny AutoMapper lub inna biblioteka wymaga decyzji licencyjnej, należy przed jej użyciem zgłosić ją Liderowi.

Starsze polecenie `dotnet new react` i literalne pliki projektu z PDF nie są wzorcem do bezpośredniego kopiowania na .NET 10. Proponujemy zachować WebStore.Web z klientem w ClientApp oraz dostosować integrację do aktualnego narzędzia budowania React, zachowując budowanie, uruchamianie i publikację klienta z serwerem. [Dokumentacja Microsoft dotycząca React i ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/client-side/spa/react?view=aspnetcore-10.0) rozróżnia obecne i starsze szablony. Jest to dostosowanie techniczne do decyzji Lidera, a nie deklaracja identyczności z historycznymi poleceniami PDF.

## Doprecyzowania zaakceptowane w v2

Reguły P-02–P-04 zostały zaproponowane przez analityka na polecenie Lidera i zaakceptowane przez Lidera wraz z v2. Stanowią uzgodnione doprecyzowania, a nie treść odczytaną z PDF.

### P-02 — encje i relacje

Id nowych encji jest typu int. Wymagane teksty nie mogą być puste ani składać się wyłącznie z białych znaków. Wartości opcjonalne są oznaczone poniżej; pozostałe pola wejściowe są wymagane.

| Encja | Proponowane pola i reguły |
| --- | --- |
| Address | Id, Country, PostalCode, City, Street, BuildingNumber, opcjonalne ApartmentNumber; AddressType = Billing, Shipping, BillingAndShipping albo Store; opcjonalne CustomerId i StationaryStoreId, z dokładnie jednym właścicielem. Brak walidacji specyficznej dla kraju w tym sprincie. |
| Customer — rozszerzenie | Kolekcja Addresses, obiektowe BillingAddress i ShippingAddress jako odwołania do wybranych własnych adresów, opcjonalne do czasu podania adresu. BillingAndShipping może być wskazany w obu rolach. Wybrane adresy muszą należeć do tego klienta i mieć odpowiedni typ. Customer może mieć wiele adresów, lecz jeden wybrany w każdej roli. |
| StationaryStore | Id, Name, opcjonalne Phone i Email; kolekcje Addresses i Employees. Sklep może mieć wiele adresów, zgodnie z literalną relacją 1:M w PDF. |
| StationaryStoreEmployee | Dziedziczenie po User; dodatkowo StationaryStoreId i Position. Jeden pracownik należy do jednego istniejącego sklepu. FirstName i LastName wymagane, RegistrationDate zapisywane przy utworzeniu. |
| Category | Id, Name i Tag. Name wymagane, Tag opcjonalne; bez dodatkowej reguły unikalności, której PDF nie określa. |
| Invoice | Id, Number, IssueDate, CustomerId, TotalAmount i kolekcja Orders. Number jest nadawany przez wywołującego i unikalny w bazie. Faktura obejmuje co najmniej jedno zamówienie jednego klienta; TotalAmount wylicza serwer jako sumę kwot tych zamówień. Bez generowania pliku PDF faktury oraz bez dodatkowych obliczeń podatkowych. |
| Order — rozszerzenie | CustomerId, opcjonalne InvoiceId i kolekcja OrderProducts. Zamówienie może jeszcze nie mieć faktury, a później należeć do jednej faktury. Jedna faktura obejmuje wiele zamówień, zgodnie z PDF. |
| OrderProduct — rozszerzenie | OrderId, ProductId, Quantity i UnitPrice (decimal). Klucz złożony OrderId + ProductId; produkt występuje najwyżej raz w danym zamówieniu, Quantity > 0. UnitPrice to cena zapamiętana przez serwer przy dodaniu pozycji, potrzebna do zachowania historycznej kwoty zamówienia. |
| ProductStock | Id, ProductId i Quantity >= 0; wiele rekordów dla produktu, bez dodawania powiązania ze sklepem, którego PDF nie wymaga. |

Zachowane relacje 1:M: Customer–Order, Supplier–Product, Product–ProductStock, Customer–Address, Category–Product, Invoice–Order, StationaryStore–Address, StationaryStore–StationaryStoreEmployee. Order–Product pozostaje N:M przez OrderProduct. Nie zamieniamy Invoice–Order na 1:1 ani StationaryStore–Address na 1:1.

### P-03 — wspólne kontrakty i usługi

Każda z czterech dodatkowych usług oferuje trzy operacje analogiczne do ProductService:

| Interfejs / klasa | Operacje | Wejście zapisu i wynik |
| --- | --- | --- |
| IOrderService / OrderService | AddOrUpdateOrder, GetOrder, GetOrders | AddOrUpdateOrderVm: Id?, CustomerId, OrderDate, DeliveryDate, TrackingNumber, lista pozycji ProductId + Quantity. OrderVm: Id, CustomerId, daty, numer śledzenia, InvoiceId?, pozycje z cenami, TotalAmount. |
| IInvoiceService / InvoiceService | AddOrUpdateInvoice, GetInvoice, GetInvoices | AddOrUpdateInvoiceVm: Id?, Number, IssueDate, lista OrderIds. InvoiceVm: Id, Number, IssueDate, CustomerId, OrderIds, TotalAmount. Klient i kwota są wyliczane z zamówień. |
| IStoreService / StoreService | AddOrUpdateStore, GetStore, GetStores | AddOrUpdateStoreVm: Id?, Name, Phone?, Email?. StoreVm: Id i te same dane oraz identyfikatory adresów i pracowników. Zapis sklepu nie zmienia kolekcji adresów i pracowników. |
| IAddressService / AddressService | AddOrUpdateAddress, GetAddress, GetAddresses | AddOrUpdateAddressVm: Id? i pola Address z tabeli P-02; dodatkowo MakeDefaultBilling i MakeDefaultShipping, domyślnie false. AddressVm: Id i dane adresu, właściciel, typ oraz informacja o wybraniu w rolach adresu klienta. |

Odczyt jednego rekordu przyjmuje wymagany predykat Expression<Func<Encja, bool>> i zwraca nullable Vm. Lista przyjmuje opcjonalny predykat; brak filtra oznacza wszystkie rekordy. Wynik zapisu zwraca odpowiadający Vm. Kolekcje wynikowe są materializowane przed zwróceniem. Usługi nie oferują usuwania w sprincie 1. Kontrakty usług nie oznaczają dodania ekranów ani endpointów biznesowych nieopisanych w PDF.

Wspólne zachowanie błędów i identyfikatorów określa P-04. Pola wymagane są sprawdzane również w usługach, ponieważ są wywoływane autonomicznie. Błędy są logowane i przekazywane wywołującemu. Walidacja odbywa się przed zmianą danych; odrzucona operacja nie pozostawia częściowo zapisanych danych. Referencje do nieistniejących encji są odrzucane. Aktualizacja zmienia dane istniejącej encji bez niejawnego tworzenia rekordów zależnych.

**Zamówienia:**
1. CustomerId wskazuje istniejącego Customer; lista pozycji jest niepusta, ProductIds nie powtarzają się i wskazują istniejące produkty. Każda Quantity jest dodatnia.
2. TotalAmount = suma Quantity × UnitPrice, z dokładnością do dwóch miejsc po przecinku. Cena produktu i zapamiętana cena pozycji mają dwie cyfry po przecinku. Klient nie podaje kwoty ani ceny pozycji.
3. Serwer zapamiętuje bieżącą Product.Price przy dodaniu pozycji. Aktualizacja ilości istniejącej pozycji zachowuje zapamiętaną cenę; nowa pozycja pobiera bieżącą cenę. Usunięcie pozycji z przekazanej listy usuwa ją z zamówienia.
4. DeliveryDate nie poprzedza OrderDate, TrackingNumber >= 0; wartość 0 oznacza brak numeru śledzenia. Brak dodatkowego workflow statusów.
5. Zapis zamówienia nie zmienia ProductStock. PDF nie wymaga rezerwacji ani realizacji wysyłki; nie dopisujemy ich do tego sprintu.
6. Zamówienia powiązanego z fakturą nie można edytować przez OrderService; zgłaszany jest InvalidOperationException. CustomerId istniejącego zamówienia jest niezmienny.

**Faktury:**
1. Lista OrderIds jest niepusta, bez duplikatów. Wszystkie zamówienia istnieją, mają tego samego klienta i nie należą do innej faktury.
2. AddOrUpdateInvoice wiąże wskazane zamówienia z fakturą i wylicza kwotę. Przy aktualizacji zastępuje listę powiązań atomowo; usunięte z listy zamówienia tracą powiązanie. Klient istniejącej faktury jest niezmienny.
3. Number nie może być puste i musi być unikalne (po usunięciu zewnętrznych spacji, bez rozróżniania wielkości liter). Brak automatycznej numeracji, płatności i stawek VAT.
4. IssueDate nie poprzedza daty żadnego z powiązanych zamówień. TotalAmount wynika z utrwalonych cen pozycji, bez przeliczania według aktualnego katalogu.

**Sklepy i adresy:**
1. StoreService zapisuje dane sklepu oraz odczytuje adresy i pracowników. Operacje tworzenia i zarządzania pracownikami nie są osobną funkcją biznesową tego sprintu; model i jego relacje są sprawdzane przez DAL i testy.
2. AddressService wymaga dokładnie jednego właściciela: Customer albo StationaryStore. Adres sklepu ma typ Store; adres klienta ma Billing, Shipping albo BillingAndShipping.
3. Właściciel istniejącego adresu jest niezmienny. Flagi MakeDefault… są dozwolone wyłącznie dla adresów klienta z odpowiednim typem. Zaznaczenie flagi przełącza daną rolę na ten adres; false nie usuwa wcześniej wybranego adresu.
4. Zmiana typu adresu wybranego w roli, z którą nowy typ jest sprzeczny, jest odrzucana. Brak wybranego adresu nie blokuje zapisu zamówienia w tym sprincie.

### P-04 — produkty i zasady błędów

1. Dla każdej operacji AddOrUpdate…: Id = null albo 0 oznacza dodanie; Id > 0 aktualizację istniejącego rekordu; Id < 0 powoduje ArgumentException. Nieistniejące dodatnie Id przy aktualizacji powoduje KeyNotFoundException, bez tworzenia nowego rekordu.
2. Null jako view model lub wymagany predykat powoduje ArgumentNullException. Nieprawidłowe pola i referencje powodują ArgumentException. Naruszenie reguły operacji, np. edycja zafakturowanego zamówienia lub zajęty numer faktury, powoduje InvalidOperationException.
3. Get… zwraca null, gdy nie znaleziono rekordu; lista bez wyników zwraca pustą kolekcję. Gdy predykat pojedynczego odczytu pasuje do wielu rekordów, zwracany jest pierwszy według rosnącego Id.
4. Produkt ma niepuste Name i Description, niepustą tablicę ImageBytes, Price >= 0 z maksymalnie dwiema cyframi po przecinku, skończone Weight >= 0 i istniejące CategoryId oraz SupplierId wskazujące odpowiedni typ użytkownika. Nie dodajemy arbitralnych limitów długości ani rozmiaru obrazu.
5. ProductVm.Quantity to suma wszystkich ProductStock.Quantity dla produktu; brak rekordów oznacza 0. Aktualizacja produktu nie zastępuje jego stanów magazynowych.
6. Zachowujemy pola i typy ProductVm oraz AddOrUpdateProductVm z PDF, a sygnaturę GetProduct oznaczamy jako nullable, zgodnie z proponowanym zachowaniem przy braku wyniku. Poprawiamy sprzeczny warunek dodawania i aktualizacji z przykładowego kodu, kierując się opisem na s. 17.
7. Testy pokrywają rozróżnienie null/0/dodatnie/ujemne Id, brak wyniku, pustą listę, walidacje, brak referencji i agregację Quantity. Dotyczy to również odpowiednich operacji pozostałych usług.

## US-01 — Gotowe środowisko pracy

**Opis:** Jako programista chcę mieć dostęp do wymaganych narzędzi, aby budować i uruchamiać WebStore na Windows.

**Źródło:** s. 2–7, rozdział 1; nadrzędne ustalenia Lidera dotyczące Git.

**Kryteria akceptacji:**
1. Dostępny jest SDK .NET w wersji rozstrzygniętej w P-01, a polecenie sprawdzające wersję potwierdza jego działanie.
2. Dostępny jest SQL Server LocalDB na Windows i można nawiązać połączenie z instancją przeznaczoną dla projektu.
3. Dostępne są VS Code i dodatki C#, SQL Server (mssql), Git Graph opisane w instrukcji albo Lider zatwierdził odstępstwo.
4. Git rozpoznaje repozytorium ZPI; istniejące ustawienia Lidera zostają zachowane.
5. Dostępne jest środowisko Node.js wymagane do budowania i uruchamiania projektu React opisanego na s. 11.

**Zależności:** P-01. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-02 — Uruchamialna solucja WebStore

**Opis:** Jako programista chcę mieć podział aplikacji na warstwy, aby rozwijać klienta i serwer z rozdzielonymi odpowiedzialnościami.

**Źródło:** s. 7–12, rozdział 2 i rozdział 3, pkt 1.

**Kryteria akceptacji:**
1. Solucja WebStore zawiera projekty WebStore.Model, WebStore.DAL, WebStore.Services, WebStore.ViewModels i WebStore.Web; projekt testowy dodawany jest w US-08.
2. Referencje odpowiadają s. 10: DAL → Model; ViewModels → Model; Services → Model, DAL, ViewModels; Web → Model, DAL, ViewModels, Services.
3. WebStore.Web obejmuje serwer ASP.NET Core i klienta React, zgodnie z decyzją P-01 dotyczącą wersji i zakresu.
4. Budowanie solucji kończy się powodzeniem, a uruchomienie WebStore.Web pozwala otworzyć działającą aplikację w przeglądarce.
5. Instrukcja uruchomienia podaje wymagania, konfigurację lokalną oraz polecenia budowania i uruchomienia, bez sekretów.

**Zależności:** US-01, P-01. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-03 — Bazowy model sklepu

**Opis:** Jako twórca usług sklepu chcę modelu użytkowników, produktów i zamówień, aby operować na danych określonych w instrukcji.

**Źródło:** s. 12, diagram UML i rozdział 3, pkt 2; s. 13, dziedziczenie TPH.

**Kryteria akceptacji:**
1. User dziedziczy po IdentityUser<int> i ma FirstName, LastName oraz RegistrationDate; Customer i Supplier dziedziczą po User.
2. Order ma Id, Customer, OrderDate, DeliveryDate, TotalAmount i TrackingNumber w typach z UML: int, Customer, DateTime, DateTime, decimal, long.
3. Product ma Id, Description, ImageBytes, Name, Price, Supplier i Weight w typach z UML: int, string, byte[], string, decimal, Supplier, float. Category zostaje zastąpiona relacją do encji kategorii w US-04.
4. ProductStock przechowuje Product i Quantity; OrderProduct przechowuje Order, Product i Quantity. Model uwzględnia powiązania zamówień klienta i produktów dostawcy.
5. Zmiany adresów, kategorii, sklepów, pracowników i faktur spełniają kryteria US-04, a ich trwałe relacje są weryfikowane w US-05.

**Zależności:** US-02; końcowy model zależy od US-04 i P-02. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-04 — Adresy, kategorie, sklepy i faktury w modelu

**Opis:** Jako twórca usług sklepu chcę oddzielnych encji adresów, kategorii, sklepów, pracowników i faktur, aby odwzorować rozszerzenia wymagane w laboratorium.

**Źródło:** s. 12, rozdział 3, pkt 3–6; s. 13; s. 18, dane Category.

**Kryteria akceptacji:**
1. Istnieje Address; adresy klienta są obiektami zamiast tekstów BillingAddress i ShippingAddress. Przeznaczenie i wybór adresu spełniają P-02 i P-03.
2. Istnieją StationaryStore i StationaryStoreEmployee; pracownik dziedziczy po User.
3. Istnieje Category powiązana z Product; uwzględnia Name i Tag użyte w przykładzie danych.
4. Istnieje Invoice i powiązanie z Order.
5. Pola, obowiązkowość i ograniczenia nowych encji oraz relacje spełniają specyfikację P-02.

**Zależności:** US-03, P-02. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-05 — Trwały model danych i migracje

**Opis:** Jako operator aplikacji chcę zapisywać dane w SQL Server, aby dane i powiązania były zachowane między uruchomieniami.

**Źródło:** s. 12–13, rozdział 4; s. 15–19, ApplicationDbContext i Identity.

**Kryteria akceptacji:**
1. Warstwa DAL udostępnia ApplicationDbContext używany przez usługi oraz mapowanie encji do SQL Server.
2. Zdefiniowane są relacje 1:M Customer–Order, Supplier–Product, Product–ProductStock i Category–Product.
3. Relacje Customer–Address, Invoice–Order, StationaryStore–Address i StationaryStore–StationaryStoreEmployee są odwzorowane zgodnie z P-02; kierunek i role adresów są jednoznaczne.
4. Relacja N:M Order–Product korzysta z encji asocjacyjnej OrderProduct i zachowuje Quantity.
5. User, Customer, Supplier i StationaryStoreEmployee są mapowane metodą TPH.
6. Utworzono migrację, zastosowano ją do lokalnej bazy projektu i sprawdzono klucze oraz powiązania. Zapisane dane można ponownie odczytać po utworzeniu nowego kontekstu.
7. Instrukcja podaje sposób utworzenia bazy i zastosowania migracji; nie wymaga usuwania danych Lidera.

**Zależności:** US-01–US-04, P-01, P-02. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-06 — Wspólny mechanizm usług i view modeli

**Opis:** Jako programista usług chcę wspólnej klasy bazowej i mapowania danych, aby implementować logikę biznesową zgodnie z architekturą instrukcji.

**Źródło:** s. 13–16 i 19, rozdział 5.1 oraz konfiguracja testów.

**Kryteria akceptacji:**
1. WebStore.Services ma katalogi Interfaces i ConcreteServices. Interfejsy usług są publiczne i stosują wzorzec I…Service.
2. Abstrakcyjny BaseService udostępnia ApplicationDbContext, IMapper i ILogger przekazane przez konstruktor.
3. View modele znajdują się w WebStore.ViewModels/VM i mają sufiks Vm.
4. Mapowania AutoMapper używane przez usługi są skonfigurowane w profilu MainProfile i pozwalają odwzorować wymagane dane wejściowe oraz wynikowe.
5. Usługi można utworzyć przez mechanizm wstrzykiwania zależności z kontekstem, mapowaniem i loggerem, jak w konfiguracji testowej z s. 19.

**Zależności:** US-02, US-05, P-01; zakres mapowań zależy od zatwierdzonych kontraktów usług. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-07 — Dodawanie, aktualizacja i odczyt produktów

**Opis:** Jako odbiorca usługi produktów chcę zapisywać i wyszukiwać produkty, aby korzystać z katalogu sklepu poprzez warstwę usług.

**Źródło:** s. 14–17, ProductVm, AddOrUpdateProductVm, IProductService i ProductService.

**Kryteria akceptacji:**
1. Istnieją interfejs IProductService oraz klasa ProductService dziedzicząca po BaseService, udostępniające AddOrUpdateProduct, GetProduct z predykatem oraz GetProducts z opcjonalnym predykatem.
2. AddOrUpdateProductVm ma Id jako int? oraz wymagane pola Description, ImageBytes, Name, Price, Weight, CategoryId i SupplierId w typach podanych na s. 14.
3. Zapis produktu utrwala dane przez kontekst i zwraca ProductVm. Rozróżnianie dodawania i aktualizacji oraz obsługa nieprawidłowych danych spełniają P-04.
4. GetProduct pobiera produkt zgodny z predykatem, a GetProducts filtruje kolekcję lub zwraca wszystkie produkty, gdy predykatu nie podano. Przy braku wyniku zwraca null, zgodnie z P-04.
5. Null przekazany jako view model do zapisu lub predykat do GetProduct powoduje ArgumentNullException; błędy usługi są logowane i przekazywane do wywołującego.
6. ProductVm zawiera Description, ImageBytes, Name, Price, Weight i Quantity. Quantity jest sumą stanów zgodnie z P-04.

**Zależności:** US-05, US-06, P-04. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-08 — Automatyczne sprawdzanie usług

**Opis:** Jako programista chcę uruchamiać powtarzalne testy usług, aby wykrywać błędy zapisu, aktualizacji i odczytu danych.

**Źródło:** s. 17–21, rozdziały 5.2–5.4.

**Kryteria akceptacji:**
1. WebStore.Tests należy do solucji i ma referencje do Services, Model oraz DAL. Używa xUnit, wstrzykiwania zależności i EF Core InMemory w wersjach zgodnych z P-01.
2. Konfiguracja testów rejestruje kontekst, Identity, mapowanie, logger i testowane usługi; dane przykładowe są gotowe przed rozpoczęciem sprawdzeń.
3. Testy ProductService sprawdzają odczyt pojedynczego produktu, listy filtrowanej, całej listy, dodawanie oraz aktualizację, wraz z rzeczywistym wynikiem i utrwalonym stanem.
4. Test pobrania całej listy porównuje wynik z przygotowanymi danymi, zamiast tautologii porównującej liczbę elementów z nią samą z przykładu na s. 20.
5. Przypadki z P-04 oraz zatwierdzone kontrakty pozostałych usług mają odpowiednie sprawdzenia. Każda z czterech usług z s. 21 ma testy swoich operacji i uzgodnionych reguł.
6. Wszystkie testy przechodzą przy zbiorczym uruchomieniu; można uruchomić wybrany test lub klasę przez filtr. Wynik nie zależy od kolejności testów i pozostałości po wcześniejszym uruchomieniu.
7. Testy InMemory nie zastępują sprawdzenia migracji i relacji na SQL Server z US-05.

**Zależności:** US-06, US-07, P-01, P-03, P-04; komplet testów zależy od US-09–US-12. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-09 — Usługa zamówień

**Opis:** Jako odbiorca usługi zamówień chcę wykonywać uzgodnione operacje na zamówieniach, aby obsługiwać je poza warstwą prezentacji.

**Źródło:** s. 21, rozdział 5.4; s. 12–13, model i relacje zamówień.

**Kryteria akceptacji:**
1. Istnieją IOrderService i OrderService dziedziczący po BaseService oraz odpowiednie view modele.
2. Operacje, dane wejściowe i wynikowe oraz zachowanie błędów spełniają kontrakt P-03 i P-04.
3. Kwota zamówienia, historyczne ceny pozycji, walidacje i brak zmiany stanów magazynowych spełniają reguły P-03.
4. Operacje i reguły mają testy uruchamiane w WebStore.Tests.

**Zależności:** US-03–US-06, US-08 (infrastruktura testów), P-02, P-03. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-10 — Usługa faktur

**Opis:** Jako odbiorca usługi faktur chcę wykonywać uzgodnione operacje na fakturach, aby zarządzać nimi przez warstwę logiki biznesowej.

**Źródło:** s. 21, rozdział 5.4; s. 12–13, Invoice i Order.

**Kryteria akceptacji:**
1. Istnieją IInvoiceService i InvoiceService dziedziczący po BaseService oraz odpowiednie view modele.
2. Operacje, dane faktury, unikalność numeru i reguły powiązania zamówień spełniają P-02–P-04.
3. Operacje, walidacje i uzgodnione sytuacje błędne mają testy w WebStore.Tests.

**Zależności:** US-04–US-06, US-08 (infrastruktura), P-02, P-03. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-11 — Usługa sklepów

**Opis:** Jako odbiorca usługi sklepów chcę wykonywać uzgodnione operacje na sklepach stacjonarnych, aby korzystać z ich danych w aplikacji.

**Źródło:** s. 21, rozdział 5.4; s. 12–13, StationaryStore i StationaryStoreEmployee.

**Kryteria akceptacji:**
1. Istnieją IStoreService i StoreService dziedziczący po BaseService oraz odpowiednie view modele.
2. Operacje i obsługiwane relacje sklepu spełniają P-02–P-04; zapis sklepu nie zmienia pracowników ani stanów.
3. Operacje, walidacje i uzgodnione sytuacje błędne mają testy w WebStore.Tests.

**Zależności:** US-04–US-06, US-08 (infrastruktura), P-02, P-03. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## US-12 — Usługa adresów

**Opis:** Jako odbiorca usługi adresów chcę wykonywać uzgodnione operacje na adresach, aby obsługiwać dane adresowe klienta i sklepu jako obiekty.

**Źródło:** s. 21, rozdział 5.4; s. 12–13, Address i relacje.

**Kryteria akceptacji:**
1. Istnieją IAddressService i AddressService dziedziczący po BaseService oraz odpowiednie view modele.
2. Operacje, wymagane pola, przypisanie do jednego właściciela, przeznaczenie i wybór adresu spełniają P-02–P-04.
3. Operacje, walidacje i uzgodnione sytuacje błędne mają testy w WebStore.Tests.

**Zależności:** US-04–US-06, US-08 (infrastruktura), P-02, P-03. **Status:** zaakceptowana przez Lidera w wersji v2 (decyzja wskazana w nagłówku).

## Pokrycie źródła i kolejność

| Część PDF | Pokrycie |
| --- | --- |
| Rozdział 1, s. 2–7 | US-01; istniejąca konfiguracja Git i nadrzędne zasady Lidera |
| Rozdział 2, s. 7–12 | US-02 |
| Rozdział 3, s. 12 | US-02–US-04 |
| Rozdział 4, s. 12–13 | US-05 |
| Rozdział 5.1, s. 13–17 | US-06–US-07 |
| Rozdziały 5.2–5.3, s. 17–21 | US-08 |
| Rozdział 5.4, s. 21 | US-09–US-12 oraz ich testy w US-08 |

Po akceptacji v2: środowisko → solucja → model → DAL i migracje → wspólne mechanizmy usług → ProductService i infrastruktura testowa → pozostałe usługi wraz z testami → przekazanie kodu Liderowi. Infrastruktura US-08 może powstać przed pozostałymi usługami; pełna realizacja US-08 wymaga ich zakończenia. Recenzent rozpoczyna pracę dopiero po akceptacji kodu przez Lidera.
