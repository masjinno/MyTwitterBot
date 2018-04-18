using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosenFlyerChecker.Model
{
    class FileCheck
    {

        public bool EqualsFiles(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) return false;
            if (path1.Equals(path2)) return true;

            using (FileStream fs1 = new FileStream(path1, FileMode.Open))
            using (FileStream fs2 = new FileStream(path2, FileMode.Open))
            {
                if (fs1.Length != fs2.Length) return false;

                int byte1;
                int byte2;
                do
                {
                    byte1 = fs1.ReadByte();
                    byte2 = fs2.ReadByte();
                    if (byte1 != byte2) return false;
                }
                while (byte1 != -1);
            }

            return true;
        }
    }
}
