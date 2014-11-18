using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary
{
    public class CommonClass
    {
        public List<CommonProperty> Properties { get; } = new List<CommonProperty>();

        public string Id { get; set; }

        public string Name { get; set; }

        public void AddProperties(params CommonProperty[] properties)
        {
            foreach (var item in properties.Except(Properties))
            {
                Properties.Add(item);
            }
        }
    }
}
