# Tartarus MUD - Textový Survival Multiplayer

Tartarus je plnohodnotná textová multiplayerová hra (MUD) s prvky survival hororu, napsaná v C# jako klientsko-serverová aplikace. Hráči se probouzejí na poškozené vesmírné stanici a musí spolupracovat (nebo bojovat o přežití), prozkoumávat okolí a nakonec stanici zachránit.

## 🚀 Hlavní funkce a implementované mechaniky

Projekt obsahuje vlastní asynchronní TCP/IP server a dedikovaného klienta, který řeší bezpečné zadávání hesel a plynulé překreslování konzole. 

### Základní (MVP) funkce:
* **Multiplayer architektura:** Asynchronní server schopný obsloužit více připojených klientů v reálném čase.
* **Perzistence a bezpečnost (I3, I4):** Hráčské účty jsou chráněny heslem (v klientovi maskováno `***`). Hra si pamatuje pozici a inventář hráčů i po jejich odpojení.
* **Data-Driven Design (I1):** Lokace (místnosti, NPC) a předměty se načítají z externích souborů `rooms.json` a `items.json`. Game design lze upravovat bez zásahu do C# kódu.

### Rozšířené herní mechaniky:
* **M11: Zamčené místnosti a klíče:** Průchod stanicí je blokován. Hráči musí nacházet přístupové karty (např. Červená karta) k odemčení nových sektorů (Auto-unlock systém).
* **M2: Soubojový systém:** Tahový souboj s NPC (Mutanti). Nepřátelé automaticky vrací úder. Systém zahrnuje HP hráče a respawn po smrti.
* **M8: Používání předmětů a zbraní:** Možnost vybavit se zbraní (Plazmový řezák) pro zvýšení poškození nebo použít lékárničku pro doplnění HP.
* **M12: Stavové efekty (Real-time Tick):** Server běží v reálném čase. Nepřátelé mohou hráči způsobit "Krvácení", které mu asynchronně (každých 10 vteřin) ubírá životy, dokud nepoužije obvaz/lékárničku.
* **M10: Příběhový Quest (P1):** Jasný cíl hry – najít fúzní baterii v laboratoři, donést ji do strojovny a restartovat hlavní generátor pro záchranu stanice.

## 🛠️ Jak hru spustit

Hra vyžaduje nainstalované prostředí **.NET SDK**.

### 1. Spuštění serveru
Server musí běžet jako první, aby se k němu mohli klienti připojit.
1. Otevřete terminál ve složce serveru (např. `GameServer`).
2. Spusťte příkaz:
   `dotnet run`
3. Server vypíše, že naslouchá na portu (výchozí `4000`) a načte data z JSON souborů.

### 2. Spuštění klienta
K serveru se může připojit libovolné množství klientů.
1. Otevřete terminál ve složce klienta (např. `TartarusClient`).
2. Spusťte příkaz:
   `dotnet run`
3. Zadejte IP adresu serveru (pro hraní na jednom PC stačí `127.0.0.1`) a port `4000`.
4. Vytvořte si postavu zadáním jména a hesla.

## 📖 Základní příkazy ve hře

* `prozkoumej` - Rozhlédne se po aktuální místnosti (ukáže východy, předměty a NPC).
* `jdi <směr>` - Přesun do jiné místnosti (např. `jdi sever`).
* `vezmi <předmět>` - Sebere předmět z místnosti do inventáře.
* `poloz <předmět>` - Zahodí předmět z inventáře na zem.
* `inventar` - Zobrazí aktuální stav HP, drženou zbraň a obsah batohu (max 5 věcí).
* `vybav <zbraň>` - Vybaví postavu zbraní z inventáře.
* `pouziji <předmět>` - Použije lékárničku (doplní HP, zastaví krvácení) nebo questový předmět.
* `utoc <cíl>` - Zaútočí na nepřítele v místnosti (např. `utoc mutant`).
* `mluv <npc>` - Promluví s postavou nebo terminálem.
* `pomoc` - Vypíše nápovědu.
* `konec` - Uloží postavu a odpojí klienta od serveru.

## 📁 Struktura datových souborů
Pokud chcete přidat novou místnost nebo předmět, upravte soubory ve složce `Data` na serveru:
* `rooms.json` - Definuje mapu, popisy, zámky na dveřích a výskyt NPC/nepřátel.
* `items.json` - Databáze předmětů. Definuje, zda je předmět zbraň (`Weapon`), lékárnička (`Consumable`), klíč (`Key`) nebo úkolový předmět (`QuestItem`), a jejich statistiky.

---
*Vytvořeno jako projekt pro SPŠE Ječná.*
