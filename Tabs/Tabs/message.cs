using System;
using System.Collections.Generic;
using System.Text;

namespace Tabs
{
    public class message
    {
        public string Name { get; set; }

        public message(string mySentence)
        {
            Name = mySentence;
        }
    }
}