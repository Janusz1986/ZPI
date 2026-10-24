# ZPI — zasady pracy w projekcie

## Cel i język projektu

Celem projektu ZPI jest dostarczenie aplikacji klient–serwer. Projekt jest realizowany iteracyjnie, w sprintach. Językiem komunikacji, analizy, user stories i raportów jest polski.

Na początku każdego sprintu Lider umieszcza dokument PDF z wymaganiami w folderze `wymagania/`. Ten dokument stanowi źródło wymagań bieżącej iteracji. Jeżeli nie wskazano dokumentu, a wybór nie jest jednoznaczny, należy zapytać Lidera. Nie wybieraj pliku wyłącznie na podstawie daty modyfikacji lub nazwy.

## Minimalne środowisko agentowe

Środowisko składa się z trzech ról opisanych w plikach:

- [analityk.md](analityk.md) — analiza PDF i przygotowanie user stories.
- [programista.md](programista.md) — implementacja full stack zaakceptowanych user stories.
- [recenzent.md](recenzent.md) — weryfikacja funkcjonalności po akceptacji kodu przez Lidera.

Role działają sekwencyjnie. Przed rozpoczęciem etapu agent czyta ten plik oraz opis swojej roli. Pliki definiują sposób pracy agentów; nie uruchamiają ich samoczynnie. Nie ma dodatkowego agenta koordynującego ani pracy równoległej. Lider zleca rozpoczęcie kolejnych etapów i podejmuje decyzje akceptacyjne.

## Przebieg sprintu i bramki akceptacji

1. Lider dostarcza PDF z wymaganiami do `wymagania/`.
2. **Analityk** analizuje wskazany dokument, wyjaśnia niejednoznaczności z Liderem i przygotowuje user stories z opisami oraz kryteriami akceptacji.
3. **Lider akceptuje user stories.** Bez tej akceptacji programista nie rozpoczyna implementacji. Przy akceptacji częściowej można realizować wyłącznie jednoznacznie wskazane, zaakceptowane stories.
4. **Programista full stack** implementuje zaakceptowany zakres, wykonuje odpowiednie sprawdzenia i przedstawia kod Liderowi.
5. **Lider akceptuje kod.** Dopiero wtedy recenzent rozpoczyna weryfikację zaimplementowanych funkcjonalności.
6. **Recenzent** sprawdza zgodność implementacji z zaakceptowanymi stories i kryteriami akceptacji oraz przedstawia wynik Liderowi.

Akceptacja musi być wyraźną decyzją Lidera odnoszącą się do konkretnej wersji i zakresu. Milczenie, upływ czasu, pozytywny wynik testów ani opinia innego agenta nie stanowią akceptacji. Agent nie może sam zatwierdzić swojego etapu ani przypisać Liderowi decyzji.

Zmiana zaakceptowanych stories wymaga ponownej akceptacji zmienionego zakresu przed jego implementacją. Poprawki kodu, także po uwagach recenzenta, wymagają ponownej akceptacji Lidera przed kolejną weryfikacją recenzenta. Recenzent zgłasza problemy, a programista wprowadza poprawki. Lider rozstrzyga o zamknięciu iteracji.

## Niejasności i blokady

Jeżeli wymaganie, kryterium akceptacji, zakres decyzji lub istotne zachowanie aplikacji jest niejednoznaczne, **zawsze zapytaj Lidera**. Opisz konkretną niejasność i jej wpływ. Nie zastępuj odpowiedzi domysłem ani samodzielnie rozszerzonym zakresem.

Wstrzymaj część pracy zależną od odpowiedzi. Możesz kontynuować niezależne, jednoznacznie określone zadania, jeśli spełniono właściwą bramkę akceptacji. Zwykłe decyzje techniczne mieszczące się w zatwierdzonych wymaganiach należą do programisty; nie mogą zmieniać zachowania wymaganego przez Lidera.

Brak PDF, brak możliwości odczytania istotnego fragmentu, brak wymaganej akceptacji lub sprzeczność źródeł należy zgłosić Liderowi. Nie deklaruj ukończenia zablokowanego zakresu.

## Dokumenty robocze i porządek

- `wymagania/` — źródłowe dokumenty PDF dostarczane przez Lidera; agenci ich nie modyfikują ani nie usuwają.
- `dokumentacja/user-stories.md` — jeden aktualny zestaw stories bieżącego sprintu, z powiązaniem ze źródłem i statusem akceptacji.
- `dokumentacja/recenzja.md` — jeden aktualny raport recenzenta dotyczący wskazanego zakresu i wersji implementacji.

Dokumenty wynikowe powstają dopiero przy wykonywaniu właściwego etapu. Zapisuj w nich nazwę źródłowego PDF oraz identyfikator sprintu podany przez Lidera; jeżeli go nie podano, identyfikuj iterację nazwą PDF, bez wymyślania numeracji sprintów.

Akceptację można otrzymać w rozmowie lub w dokumencie od Lidera. W aktualnym dokumencie zanotuj jej zakres i odniesienie do zatwierdzonej wersji, tylko na podstawie rzeczywistej decyzji. Każda zmiana treści merytorycznej unieważnia akceptację zmienionego zakresu. Wersje dokumentu oznaczaj kolejno, np. `v1`, `v2`, w jego nagłówku; nie twórz osobnych kopii pliku.

**Nie pozostawiaj artefaktów redakcyjnych pomiędzy iteracjami:** usuń własne pliki tymczasowe, puste szablony robocze, duplikaty, nieaktualne warianty i komentarze redakcyjne. Aktualizuj dokumenty w miejscu, bez kopii typu `final2` czy `backup`. Przy rozpoczęciu kolejnej iteracji zastąp treść dokumentów bieżących, usuwając nieaktualny raport i stare statusy akceptacji. Nie przenoś akceptacji między sprintami. Zachowaj końcowe wyniki bieżącej iteracji do czasu rozpoczęcia następnej.

Nie usuwaj cudzych plików, kodu, testów ani źródłowych wymagań w ramach porządkowania. Jeśli Lider wymaga archiwum, uzgodnij jego miejsce i zakres przed zastąpieniem dokumentów.

## Kod i repozytorium

**Nie komituj kodu ani dokumentacji. Commitowanie jest wyłączną rolą Lidera.** Nie wykonuj również push, merge ani operacji publikujących zmiany bez odrębnego polecenia Lidera. Pozostaw przygotowane zmiany do jego przeglądu.

Nie nadpisuj ani nie wycofuj zmian Lidera. Przed edycją zapoznaj się z istniejącym kodem i instrukcjami projektu. Stos technologiczny nie został jeszcze ustalony; jeżeli repozytorium go nie określa, uzgodnij go z Liderem przed rozpoczęciem implementacji.

Raportuj wykonany zakres, przeprowadzone sprawdzenia, ich rzeczywiste wyniki oraz blokady. Nie twierdź, że test przeszedł, jeśli nie został uruchomiony. Nie zapisuj sekretów w kodzie ani dokumentacji.
