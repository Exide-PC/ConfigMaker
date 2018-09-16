using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class BuyMenuModel: EntryModel
    {
        public List<CategoryModel> Categories { get; } = new List<CategoryModel>();
    }
}
