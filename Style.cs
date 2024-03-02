namespace ModHearth
{
    /// <summary>
    /// Central style area
    /// </summary>
    public static class Style
    {
        // ModReferencePanel style.
        public static Color modRefColor = Color.Gray;
        public static BorderStyle modRefBorder = BorderStyle.None;
        public static Font modRefFont = new Font("Arial", 9, FontStyle.Bold);
        public static int modRefHeight = 16;
        public static Padding modRefPadding = new Padding(2, 1, 2, 0);

        // Main form style.
        public static int largeBorder = 12;
        public static int smallBorder = 6;

        // Right panel style.
        public static int rightPanelW = 120;

        // Left panel style.
        // The width and height of a standard steam workshop item image. 
        public static int leftPanelW = 636;
        public static int leftPanelH = 358;
    }
}
