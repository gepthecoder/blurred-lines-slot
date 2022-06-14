
namespace SlotHelper
{
    public static class Consts
    {
        public const string spin_button_trigger_ = "rotate";

        public const string auto_spin_text_normal_ = "AUTO";
        public const string auto_spin_text_stop_ = "STOP";

    }

    public static class Enums
    {
        [System.Serializable]
        public enum Symbol
        {
            None = 0,

            Cherry = 1,
            Apple = 2,
            Pear = 3,
            Orange = 4,
            Grapes = 5,
            Watermelon = 6,
            Blueberry = 7,
            Strawberry = 8,
            Wild = 9,
            Scatter = 10,
        }

        public static Symbol StringToSymbol(string symbol)
        {
            switch (symbol)
            {
                case "Cherry":
                    return Symbol.Cherry;
                case "Apple":
                    return Symbol.Apple;
                case "Pear":
                    return Symbol.Pear;
                case "Orange":
                    return Symbol.Orange;
                case "Grapes":
                    return Symbol.Grapes;
                case "Watermelon":
                    return Symbol.Watermelon;
                case "Blueberry":
                    return Symbol.Blueberry;
                case "Strawberry":
                    return Symbol.Strawberry;
                case "Wild":
                    return Symbol.Wild;
                case "Scatter":
                    return Symbol.Scatter;
                default:
                    return Symbol.None;
            }
        }


        public static int SymbolToId(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.Cherry:
                    return 1;
                case Symbol.Apple:
                    return 2;
                case Symbol.Pear:
                    return 3;
                case Symbol.Orange:
                    return 4;
                case Symbol.Grapes:
                    return 5;
                case Symbol.Watermelon:
                    return 6;
                case Symbol.Blueberry:
                    return 7;
                case Symbol.Strawberry:
                    return 8;
                case Symbol.Wild:
                    return 9;
                case Symbol.Scatter:
                    return 10;
                case Symbol.None:
                default:
                    return -1;
            }
        }
    }

}
