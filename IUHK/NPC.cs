namespace TartarusMUD.Models
{
    public class Npc
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Dialogue { get; set; }
        
        // Bojové vlastnosti
        public int Hp { get; set; }
        public int Damage { get; set; }
        public bool IsHostile { get; set; } // Určuje, zda vrací útok
    }
}