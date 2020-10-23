using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteSafe.Classes
{
    public class Category
    {
        private readonly String name;
        private readonly int id;

        public int ID
        {
            get { return this.id; }
        }

        public String Name
        {
            get { return this.name; }
        }

        public Category(int id, String input)
        {
            this.id = id;
            this.name = input;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
