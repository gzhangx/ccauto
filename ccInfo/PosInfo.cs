

namespace ccInfo
{
    public class ccPoint
    {
        public int x;
        public int y;
        public ccPoint(int xx, int yy)
        {
            x = xx;
            y = yy;
        }
    }

    public class NameLevel
    {
        public string name;
        public string level { get; set; }
    }
    public class PosInfo
    {
        public NameLevel nameLevel;
        public string Name
        {
            get
            {
                return nameLevel?.name;
            }
        }
        public int Level
        {
            get
            {
                int res = 999;
                if (nameLevel == null) return res;
                int.TryParse(nameLevel.level, out res);
                return res;
            }
        }
        public ccPoint point;
    }
}
