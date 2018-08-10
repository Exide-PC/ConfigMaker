using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class WeaponCategoryModel: EntryModel
    {
        public List<EntryModel> Weapons { get; } = new List<EntryModel>();
        public string Name { get; set; }
    }
}
