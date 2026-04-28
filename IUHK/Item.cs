namespace TartarusMUD.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
        // Typ předmětu: "Key" (klíč), "Consumable" (lékárnička), "Weapon" (zbraň)
        public string Type { get; set; }
        
        // Vlastnosti (pro klíče zůstanou na 0)
        public int HealAmount { get; set; }
        public int DamageBonus { get; set; }
    }
}