using System.Collections.Generic;
using TartarusMUD.Models;

namespace TartarusMUD.Core
{
    public class World
    {
        public Room StartRoom { get; private set; }

        public World()
        {
            BuildWorld();
        }

        private void BuildWorld()
        {
            // 1. Vytvoření místností
            Room cryo = new Room("kryo_1", "Kryogenická komora A", "Probudil ses v chladné, matně osvětlené místnosti. Všude kolem jsou poškozené kryokapsle. Na sever vedou pootevřené dveře.");
            Room corridor = new Room("chodba_1", "Temná chodba", "Dlouhá kovová chodba. Blikající zářivky odhalují krvavé stopy na podlaze. Na jihu je kryokomora, na východě vidíš skladiště.");
            Room storage = new Room("sklad_1", "Skladiště", "Místnost plná rozházených beden a harampádí. Vypadá to, že tu někdo ve spěchu hledal zásoby. Na západ se vrátíš do chodby.");

            // 2. Propojení východů
            cryo.Exits.Add("sever", corridor);
            corridor.Exits.Add("jih", cryo);
            corridor.Exits.Add("vychod", storage);
            storage.Exits.Add("zapad", corridor);

            // 3. Přidání předmětů (zatím jen textové řetězce)
            cryo.Items.Add("karta"); // Přístupová karta
            storage.Items.Add("lekarnicka");

            // 4. Přidání NPC pro příkaz "mluv"
            corridor.Npcs.Add("bot", "Údržbářský bot jiskří a mechanickým hlasem opakuje: 'Kritické poškození trupu. Sektor uzavřen.'");

            // Nastavení startovní místnosti
            StartRoom = cryo;
        }
    }
}